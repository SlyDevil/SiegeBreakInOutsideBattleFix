using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static TaleWorlds.CampaignSystem.MapEvents.MapEvent;

namespace SiegeBreakInOutsideBattleFix
{
    [HarmonyPatch]
    class Patches
    {
        //prefix to find the state of the siege as player is being added to the map event
        [HarmonyPatch(typeof(MapEvent), "AddInvolvedPartyInternal")]
        static void Prefix (MapEvent __instance, PartyBase involvedParty, BattleSideEnum side)
        {
            //AddInvolvedPartyInternal
            //AIPI is called every time a party is assigned to a side
            //when the player clicks on a town under siege of their faction, the player is assigned to the defender side which then calls AIPI and if there is an on-going siege assault, then it sets the map event type to SiegeOutside
            //even if the player then breaks in, the map event has already been changed to Outside and so the player will attempt to join a siege defence on the walls and instead fight a field battle
            //oddly, if you click on the siege camp and break in, it properly loads a wall defence
            //I'm unsure why, but I'd guess it's because clicking on the besieger camp makes the player party the lead party of a 2nd map event outside with the garrison and militia as the additional InvolvedParties which means that those parties will fail to pass the mobileparty.CurrentSettlement == null check and therefore not have the map even type changed from Siege to SiegeOutside
            //ideally, AIPI shouldn't be setting the battle type at all, that should be a consequence of the menu options selected for all of the siege related map events
            //and ideallylly, AIPI shouldn't already be adding the player to the map event because choosing the Leave option from the join_siege menu will throw the player into a battle menu despite them having just chosen to not be involved in the event

            //grab the event type before AIPI changes it so that when the break in choice is made, I can revert the change to the correct event type

            InformationManager.DisplayMessage(new InformationMessage("Event Type before AIPI : " + __instance.EventType, Colors.Green));
            battleTypeBeforePartyAdded = __instance.EventType;
        }

        [HarmonyPatch(typeof(MapEvent), "AddInvolvedPartyInternal")]
        static void Postfix(MapEvent __instance, PartyBase involvedParty, BattleSideEnum side)
        {
            InformationManager.DisplayMessage(new InformationMessage("Event Type after AIPI : " + __instance.EventType, Colors.Red));
        }

        //change the consequence of break in to set the map event type back to its prior state
        [HarmonyPatch(typeof(EncounterGameMenuBehavior), "break_in_debrief_continue_on_consequence")]
        static void Prefix()
        {
            //BreakInDebriefContinueConsequence
            //_mapEventType is a private field
            //this is null when running in native; the player event isn't actually started until they have hit continue in the "break_in_debrief" menu which means that native isn't the issue
            //the postfix is probably the issue, but I also am not calling the correct map event which is probably why it's null
            //ToR is probably having an issue with the Postfix on encounter Init which sets a player side and therefore causes a call to AIPI
            MapEvent playerMapEvent = MapEvent.PlayerMapEvent;
            //Assembly assem = typeof(MapEvent).Assembly;
            //playerMapEvent
            if (playerMapEvent != null)
            {
                InformationManager.DisplayMessage(new InformationMessage("Player map event : " + playerMapEvent.ToString(), Colors.Cyan));
                FieldInfo playerMapEventTypeField = typeof(MapEvent).GetField("_mapEventType", BindingFlags.Instance | BindingFlags.NonPublic);
                //I should add some checks for the type actually being a siege or something
                //if (playerMapEvent.EventType != null) 
                //{
                    InformationManager.DisplayMessage(new InformationMessage("Event Type before I change it : " + playerMapEvent.EventType.ToString(), Colors.Yellow));
                    playerMapEventTypeField.SetValue(playerMapEvent, battleTypeBeforePartyAdded);
                    //var playerMapEventInstance = playerMapEvent;
                    InformationManager.DisplayMessage(new InformationMessage("Event Type after I change it : " + playerMapEvent.EventType.ToString(), Colors.Magenta));
                //}
            }
            
        }

        public static BattleTypes battleTypeBeforePartyAdded = BattleTypes.None;
    }
}
