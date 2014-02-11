//              ________________________________
//             /                               /
//            /       Slayer_Infection v1     /
//           /          HellsHero  1786      /
//          /_______________________________/
//      __ __    __  __      ____      __  __                
//     / // /   / / / /___  / / /_____/ / / /___  _________ 
//    / // /   / /_/ // _ \/ / // ___/ /_/ // _ \/ ___/ __ \ 
//   / // /   / __  //  __/ / /(__  ) __  //  __/ /  / /_/ / 
//  /_//_/   /_/ /_/ \___/_/_//____/_/ /_/ \___/_/   \____/

//

if(!$Slayer::Server::Dependencies::Gamemodes)
	exec("Add-ons/Gamemode_Slayer/Dependencies/Gamemode.cs");
Slayer.Gamemodes.addMode("Infection","INF",1,1);

if(!$Slayer::Server::Dependencies::Preferences)
	exec("Add-ons/Gamemode_Slayer/Dependencies/Preferences.cs");
//ayer.Prefs.addPref(Pref Title,Category,Variable,Type,Default Value,Requires Reset,Notify Players,Admin Level,List,Callback,Preload);
Slayer.Prefs.addPref("INF","Only team 1 can switch players","%mini.oneTeam","bool",0,1,1,-1,"Rules INF Mode");
Slayer.Prefs.addPref("INF","Deaths until Player switches team","%mini.deathCountToSwitch","int 1 100",1,1,1,-1,"Rules INF Mode");
Slayer.Prefs.addPref("INF","Switch killed player on friendly fire","%mini.friendlyFireSwitch","bool",0,0,1,-1,"Rules INF Mode");
Slayer.Prefs.addPref("INF","End Round when a team is empty","%mini.endRoundOnEmptyTeam","bool",0,1,1,-1,"Rules INF Mode");

Slayer.Prefs.addPref("INF","Switch Grace Period","%mini.player_switchTimeLimit","int 0 120",0,0,1,-1,"Rules INF Player");

Slayer.Prefs.addPref("INF","Switched Team Penalty","%mini.respawn_switchPenalty","int 0 30",0,0,1,-1,"Rules INF Respawn");

Slayer.Prefs.addPref("INF","Switched a Player","%mini.points_switchPoints","int -999 999",1,0,1,-1,"Rules INF Points");







