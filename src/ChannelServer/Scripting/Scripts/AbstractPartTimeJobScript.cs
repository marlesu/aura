using Aura.Mabi;
using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Scripting.Scripts
{
    abstract class AbstractPartTimeJobScript : GeneralScript
    {
        public abstract PtjType JobType { get; }
           
        public abstract int Start { get; }
        public abstract int Report { get; }
        public abstract int Deadline { get; }
        public abstract int PerDay { get; }

        public abstract int[] QuestIds { get; }

        public abstract string NpcNameId { get; }

        const string HOOK_NAME_AFTER_INTRO = "after_intro";
        const string HOOK_NAME_BEFORE_KEYWORDS = "before_keywords";

        int Remaining { get; set; }

        [On("ErinMidnightTick")]
        public void OnErinnMidnightTick(ErinnTime time)
        {
            // Reset available jobs at the end of the day
            Remaining = PerDay;
        }

        public override void Load()
        {
            AddHook(NpcNameId, HOOK_NAME_AFTER_INTRO, AfterIntro);
            AddHook(NpcNameId, HOOK_NAME_BEFORE_KEYWORDS, BeforeKeywords);
        }

        public async Task<HookResult> AfterIntro (NpcScript npc, params object[] args)
        {
            // Call PJ method after intro if it's time to report
            if (npc.DoingPtjForNpc() && npc.ErinnHour(Report, Deadline))
            {
                await AboutArbeit(npc);
                return HookResult.Break;
            }

            return HookResult.Continue;
        }

        public async Task<HookResult> BeforeKeywords(NpcScript npc, params object[] args)
        {
            var keyword = args[0] as string;

            // Hook PTJ keyword
            if (keyword == "about_arbeit")
            {
                await AboutArbeit(npc);
                await npc.Conversation();
                npc.End();

                return HookResult.End;
            }

            return HookResult.Continue;
        }

        /// <summary>
        /// Uses externalized strings paired with a key to say the npc lines.
        /// All the npcs have the exact same conversation trees just with different lines.
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public async Task AboutArbeit (NpcScript npc)
        {
            // Check if already doing another PTJ
            if (npc.DoingPtjForOtherNpc())
            {
                
                npc.Msg(L(NpcDialogs.doingPtjForAnotherNpc));
                return;
            }

            // Check if PTJ is in progress
            if (npc.DoingPtjForNpc())
            {
                var result = npc.GetPtjResult();

                // Check if report time
                if (!npc.ErinnHour(Report, Deadline))
                {
                    if (result == QuestResult.Perfect)
                        npc.Msg(L("Ah, you are here already?<br/>It's a little bit too early. Can you come back around the deadline?"));
                    else
                        npc.Msg(L("I hope you didn't forget what I asked you to do.<p/>Please have it done by the deadline."));
                    return;
                }

                // Report?
                npc.Msg(L("Did you complete the task I requested?<br/>You can report now and finish it up,<br/>or you may report it later if you're not done yet."), npc.Button(L("Report Now"), "@report"), npc.Button(L("Report Later"), "@later"));

                if (await npc.Select() != "@report")
                {
                    npc.Msg(L("Please report before the deadline is over.<br/>Even if the work is not done, you should still report.<br/>Then I can pay you for what you've completed."));
                    return;
                }

                // Nothing done
                if (result == QuestResult.None)
                {
                    npc.GiveUpPtj();

                    npc.Msg(npc.FavorExpression(), L("Ha ha. This is a little disappointing.<br/>I don't think I can pay you for this."));
                    npc.ModifyRelation(0, -Random(3), 0);
                }
                // Low~Perfect result
                else
                {
                    npc.Msg(L("You are quite skillful, <username/>.<br/>Now there's nothing to worry about even if I get too much work. Ha ha.<br/>Please choose what you want. You deserve it.<br/>I'd like to give it to you as a compensation for your hard work."), npc.Button(L("Report Later"), "@later"), npc.PtjReport(result));
                    var reply = await npc.Select();

                    // Report later
                    if (!reply.StartsWith("@reward:"))
                    {
                        npc.Msg(L("Please report before the deadline is over.<br/>Even if the work is not done, you should still report.<br/>Then I can pay you for what you've completed."));
                        return;
                    }

                    // Complete
                    npc.CompletePtj(reply);
                    Remaining--;

                    // Result msg
                    if (result == QuestResult.Perfect)
                    {
                        npc.Msg(npc.FavorExpression(), L("Great! You have done well as I requested.<br/>I hope you can help me again next time."));
                        npc.ModifyRelation(0, Random(3), 0);
                    }
                    else if (result == QuestResult.Mid)
                    {
                        npc.Msg(npc.FavorExpression(), L("Thank you. Although you didn't complete the job, you've done enough so far.<br/>But I'm sorry to tell you I must deduct a little from your pay."));
                        npc.ModifyRelation(0, Random(1), 0);
                    }
                    else if (result == QuestResult.Low)
                    {
                        npc.Msg(npc.FavorExpression(), L("Hmm... It's not exactly what I expected, but thank you.<br/>I'm afraid this is all I can pay you."));
                        npc.ModifyRelation(0, -Random(2), 0);
                    }
                }
                return;
            }

            // Check if PTJ time
            if (!npc.ErinnHour(Start, Deadline))
            {
                npc.Msg(L("Hmm... It's not a good time for this.<br/>Can you come back when it is time for part-time jobs?"));
                return;
            }

            // Check if not done today and if there are jobs remaining
            if (!npc.CanDoPtj(JobType, Remaining))
            {
                npc.Msg(L("I'm all set for today.<br/>Will you come back tomorrow?"));
                return;
            }

            // Offer PTJ
            var randomPtj = npc.RandomPtj(JobType, QuestIds);

            // Msg is kinda unofficial, she currently says the following, and then
            // tells you you'd get Homestead seeds.
            npc.Msg(L("Are you here for a part-time job at my Inn again?"), npc.PtjDesc(randomPtj, L("Piaras's Inn Part-time Job"), L("Looking for help with delivering goods to Inn."), PerDay, Remaining, npc.GetPtjDoneCount(JobType)));

            if (await npc.Select() == "@accept")
            {
                npc.Msg(L("I'll be counting on you as usual."));
                npc.StartPtj(randomPtj);
            }
            else
            {
                npc.Msg(L("You want to sleep on it?<br/>Alright, then.<br/>But report on time please."));
            }
        }
    }
}
