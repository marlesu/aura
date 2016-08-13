using Aura.Mabi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Scripting.Scripts
{
    abstract class AbstractPartTimeJobScript : GeneralScript
    {
        
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
            //AddHook(NpcNameId, HOOK_NAME_AFTER_INTRO, 
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
            throw new NotImplementedException();
        }
    }
}
