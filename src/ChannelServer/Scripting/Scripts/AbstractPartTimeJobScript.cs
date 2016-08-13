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

        // The npc name id that is taken as a key to hook this script to ptj keyword. eg. _piaras
        public abstract string NpcNameId { get; }
        // The name of the part time job that is shown to the user. eg. Piaras's Inn Part-time Job
        public abstract string PtjName { get; }
        // The description of the part time job that is shown to the user. eg. Looking for help with delivering goods to Inn.
        public abstract string PtjDescription { get; }

        const string HOOK_NAME_AFTER_INTRO = "after_intro";
        const string HOOK_NAME_BEFORE_KEYWORDS = "before_keywords";

        int Remaining { get; set; }

        // Must set all of these strings in the struct
        struct Dialogs
        {
            public readonly string doingPtjForAnotherNpc;

            public readonly string earlyReport_finished;
            public readonly string earlyReport_notFinished;

            public readonly string report_dialogText;
            public readonly string report_cancelReport;
            public readonly string report_noResult;
            public readonly string report_result_dialogText;
            public readonly string report_result_cancelReport;
            public readonly string report_result_perfectResult;
            public readonly string report_result_midResult;
            public readonly string report_result_lowResult;

            public readonly string lateToAcceptPtj;
            public readonly string noMorePtjLeft;

            public readonly string askPtj_dialogText;
            public readonly string askPtj_accept;
            public readonly string askPtj_reject;

        }
        
        Dialogs PtjDialogs { get; }

        protected AbstractPartTimeJobScript()
        {
            PtjDialogs = new Dialogs();
        }

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
                
                npc.Msg(L(PtjDialogs.doingPtjForAnotherNpc));
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
                        npc.Msg(L(PtjDialogs.earlyReport_finished));
                    else
                        npc.Msg(L(PtjDialogs.earlyReport_notFinished));
                    return;
                }

                // Report?
                npc.Msg(L(PtjDialogs.report_dialogText), npc.Button(L("Report Now"), "@report"), npc.Button(L("Report Later"), "@later"));

                if (await npc.Select() != "@report")
                {
                    npc.Msg(L(PtjDialogs.report_cancelReport));
                    return;
                }

                // Nothing done
                if (result == QuestResult.None)
                {
                    npc.GiveUpPtj();

                    npc.Msg(npc.FavorExpression(), L(PtjDialogs.report_noResult));
                    npc.ModifyRelation(0, -Random(3), 0);
                }
                // Low~Perfect result
                else
                {
                    npc.Msg(L(PtjDialogs.report_result_dialogText), npc.Button(L("Report Later"), "@later"), npc.PtjReport(result));
                    var reply = await npc.Select();

                    // Report later
                    if (!reply.StartsWith("@reward:"))
                    {
                        npc.Msg(L(PtjDialogs.report_result_cancelReport));
                        return;
                    }

                    // Complete
                    npc.CompletePtj(reply);
                    Remaining--;

                    // Result msg
                    if (result == QuestResult.Perfect)
                    {
                        npc.Msg(npc.FavorExpression(), L(PtjDialogs.report_result_perfectResult));
                        npc.ModifyRelation(0, Random(3), 0);
                    }
                    else if (result == QuestResult.Mid)
                    {
                        npc.Msg(npc.FavorExpression(), L(PtjDialogs.report_result_midResult));
                        npc.ModifyRelation(0, Random(1), 0);
                    }
                    else if (result == QuestResult.Low)
                    {
                        npc.Msg(npc.FavorExpression(), L(PtjDialogs.report_result_lowResult));
                        npc.ModifyRelation(0, -Random(2), 0);
                    }
                }
                return;
            }

            // Check if PTJ time
            if (!npc.ErinnHour(Start, Deadline))
            {
                npc.Msg(L(PtjDialogs.lateToAcceptPtj));
                return;
            }

            // Check if not done today and if there are jobs remaining
            if (!npc.CanDoPtj(JobType, Remaining))
            {
                npc.Msg(L(PtjDialogs.noMorePtjLeft));
                return;
            }

            // Offer PTJ
            var randomPtj = npc.RandomPtj(JobType, QuestIds);

            // Msg is kinda unofficial, she currently says the following, and then
            // tells you you'd get Homestead seeds.
            npc.Msg(L(PtjDialogs.askPtj_dialogText), npc.PtjDesc(randomPtj, L(PtjName), L(PtjDescription), PerDay, Remaining, npc.GetPtjDoneCount(JobType)));

            if (await npc.Select() == "@accept")
            {
                npc.Msg(L(PtjDialogs.askPtj_accept));
                npc.StartPtj(randomPtj);
            }
            else
            {
                npc.Msg(L(PtjDialogs.askPtj_reject));
            }
        }
    }
}
