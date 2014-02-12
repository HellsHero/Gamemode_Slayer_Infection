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
Slayer.Prefs.addPref("INF","End Round when","%mini.endRoundOnEmptyTeam","list" TAB "0 All but one team becomes empty" TAB "1 A team becomes empty",0,1,1,-1,"Rules INF Mode");
Slayer.Prefs.addPref("INF","Switch Grace Period","%mini.player_switchTimeLimit","int 0 999",0,0,1,-1,"Rules INF Player");
Slayer.Prefs.addPref("INF","Switched Team Penalty","%mini.respawnPenalty_switch","int 0 999",0,0,1,-1,"Rules INF Respawn","%mini.updateRespawnTime(switchRespawnPenalty,%1,%2);");
Slayer.Prefs.addPref("INF","Switched a Player","%mini.points_switchPoints","int -999 999",1,0,1,-1,"Rules INF Points");



//Functions to be used:
//Slayer_INF_preDeath(%mini,%client,%obj,%killer,%type,%area)   //Check for %mini.oneTeam, %mini.friendfireswitch, %client.switchGrace. after add client deathcount, if deathcount gets to deathcounttoswitch, set deathcount to 0 and switch client team and %client.switchgrace = getsimtime() and increment %killer's score
                                                                //If(((%mini.oneTeam && %killer.team == 0) || (!%mini.oneTeam) || (%client.team == %killer.team && %mini.friendlyFireSwitch)) && (getSimTime()-%client.switchGrace > %mini.player_switchTimeLimit)) ->
                                                                //{ Add to clients deathcount, if deathcount reaches deathCountToSwitch ->
                                                                //[ set deathcount to 0 and switch client's team and %client.switchGrace = getSimTime() ] }
                                                                
//Slayer_INF_Teams_onLeave(%mini,%team,%client)                 //check if the team is now empty, if it is, check victory conditions

//Slayer_INF_Teams_onJoin(%mini,%team,%client)

//Slayer_INF_onLeave(%mini,%client)



function Slayer_INF_preDeath(%mini,%client,%obj,%killer,%type,%area)// %mini.friendfireswitch, %client.switchGrace. after add client deathcount, if deathcount gets to deathcounttoswitch, set deathcount to 0 and switch client team and %client.switchgrace = getsimtime() and increment %killer's score
{
    if(%killerTeam = %killer.getTeam() != Slayer.Teams.getObject(0) && %mini.oneTeam)
        return;
    if(%mini.friendlyFireSwitch || (!%mini.friendlyFireSwitch && %teamStatus = (%clientTeam = %client.getTeam() != %killerTeam))) //pass this if. friendly fire is enabled or friendly fire is disabled & teams aren't friendly
    {
        if(%time = getSimTime()-%client.switchGrace > %mini.player_switchTimeLimit*1000) //pass this if. grace time is passed (in ms)
        {
            if(%client.deathCountToSwitch++ >= %mini.deathCountToSwitch) //inc deathCount. pass this if. deathcount threshold reached
            {
                %client.deathCountToSwitch = 0;
                echo(getSimTime() SPC %time);
                %client.switchGrace = %time;
                if(!%teamStatus) //pass this if. friendly fire is enabled
                    Slayer_INF_switchTeam(%mini,%client,-1,%clientTeam);
                else
                {
                    %killer.incScore(%mini.points_switchPoints);
                    Slayer_INF_switchTeam(%mini,%client,%killerTeam,%clientTeam);
                }
            }
        }
    }
}

function Slayer_INF_switchTeam(%mini,%client,%newTeam,%oldTeam)
{
    if(%mini.getLives() > 0)
        %client.tempLives = %client.getLives();
    if(%newTeam != -1)
        %newTeam.addMember(%client,"raison test",1);
    else
    {
        %teams = %mini.Teams;
        %teamCount = %teams.getCount();
        for(%i=0;%i<%teamCount;%i++)
        {
            if(%teams.getObject(%i) == %oldTeam)
                continue;
            if(%teamList) $= "")
                %teamList = %teams.getObject(%i) TAB;
            else
                %teamList = %teamList TAB %teams.getObject(%i);
        }
        %newTeam = getField(%teamList,getRandom(0,%teamCount-1));
        %newTeam.addMember(%client,"raison random test",1);
    }
}

function Slayer_INF_Teams_onJoin(%mini,%team,%client) //keep lives constant between team switches
{
    if(%mini.getLives() > 0 && %client.tempLives >= 0)
    {
        %client.setLives(%client.tempLives);
        %client.tempLives = "";
    }
}

function Slayer_INF_Teams_onLeave(%mini,%team,%client) //call function for when the client has left the team
{
    echo(%team.numMembers SPC "Member COUNT");
    schedule(1,0,Slayer_INF_postLeave(%mini,%team,%client));
}

function Slayer_INF_postLeave(%mini,%team,%client) //check if a team is empty, if so, end round
{
    echo(%team.numMembers SPC "Member COUNT after");
    if(%team.numMembers == 0 || !%team.getLiving())
    {
        %teams = %mini.Teams;
        if(%teamCount = %teams.getCount() == 2) //All but one team && A team becomes empty
            %mini.endRound((((%a = %mini.Teams.getObject(0)) == %team) ? %mini.Teams.getObject(1) : %a));
        else
        {
            if(!%mini.endRoundOnEmptyTeam)
            {
                //check teams for life and membership
                //if only one team has living members it is victorious
            }
            else
            {
                //get name of teams with players still living and with members
                //make those teams victorious
            }
        }

    }
}

