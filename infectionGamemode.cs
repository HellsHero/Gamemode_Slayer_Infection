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

if(!$Slayer::Server::Dependencies::Gamemodes)
	exec("Add-ons/Gamemode_Slayer/Dependencies/Gamemode.cs");
Slayer.Gamemodes.addMode("Infection","INF",1,1);

if(!$Slayer::Server::Dependencies::Preferences)
	exec("Add-ons/Gamemode_Slayer/Dependencies/Preferences.cs");
Slayer.Prefs.addPref("INF","Only team 1 can Infect players","%mini.oneTeam","bool",0,1,1,-1,"Rules INF Mode");
Slayer.Prefs.addPref("INF","Deaths until player Infects","%mini.deathCountToSwitch","int 1 100",1,1,1,-1,"Rules INF Mode");
Slayer.Prefs.addPref("INF","Reverse Infect teamkilled player","%mini.friendlyFireSwitch","bool",0,0,1,-1,"Rules INF Mode");
Slayer.Prefs.addPref("INF","End Round when","%mini.endRoundOnEmptyTeam","list" TAB "0 All but one team becomes empty" TAB "1 A team becomes empty",0,1,1,-1,"Rules INF Mode");
Slayer.Prefs.addPref("INF","Infect-proof Time","%mini.player_switchTimeLimit","int 0 999",0,0,1,-1,"Rules INF Player");
Slayer.Prefs.addPref("INF","Infected Penalty","%mini.respawnPenalty_switch","int 0 999",0,0,1,-1,"Rules INF Respawn","%mini.updateRespawnTime(switchRespawnPenalty,%1,%2);");
Slayer.Prefs.addPref("INF","Infected a Player","%mini.points_switchPoints","int -999 999",1,0,1,-1,"Rules INF Points");

function Slayer_INF_postDeath(%mini,%client,%obj,%killer,%type,%area)// %mini.friendfireswitch, %client.switchGrace. after add client deathcount, if deathcount gets to deathcounttoswitch, set deathcount to 0 and switch client team and %client.switchgrace = getsimtime() and increment %killer's score
{
    %killerTeam = %killer.getTeam();
    if(%killerTeam != %mini.Teams.getObject(0) && %mini.oneTeam)
        return;
    %clientTeam = %client.getTeam();
    %teamStatus = (%clientTeam != %killerTeam);
    if(%mini.friendlyFireSwitch || (!%mini.friendlyFireSwitch && %teamStatus)) //pass this if. friendly fire is enabled or friendly fire is disabled & teams aren't friendly
    {
        %time = getSimTime();
        if(%time-%client.switchGrace > %mini.player_switchTimeLimit*1000) //pass this if. grace time is passed (in ms)
        {
            if(%client.deathCountToSwitch++ >= %mini.deathCountToSwitch) //inc deathCount. pass this if. deathcount threshold reached
            {
                %client.deathCountToSwitch = 0;
                %client.switchGrace = %time;
                if(!%teamStatus) //pass this if. friendly fire is enabled
                    schedule(10,0,Slayer_INF_switchTeam,%mini,%client,-1,%clientTeam);
                else
                {
                    %killer.incScore(%mini.points_switchPoints);
                    schedule(10,0,Slayer_INF_switchTeam,%mini,%client,%killerTeam,%clientTeam);
                }
            }
        }
    }
}

function Slayer_INF_switchTeam(%mini,%client,%newTeam,%oldTeam)
{
    if(%mini.lives > 0)
        %client.tempLives = %client.getLives();
    if(%newTeam != -1)
        %newTeam.addMember(%client,"Infected",1);
    else
    {
        %teams = %mini.Teams;
        %teamCount = %teams.getCount();
        for(%i=0;%i<%teamCount;%i++)
        {
            if(%teams.getObject(%i) == %oldTeam)
                continue;
            if(%teamList $= "")
                %teamList = %teams.getObject(%i);
            else
                %teamList = %teamList TAB %teams.getObject(%i);
        }
        %newTeam = getField(%teamList,getRandom(0,%teamCount-2));
        %newTeam.addMember(%client,"Reverse Infection",1);
    }
}

function Slayer_INF_Teams_onJoin(%mini,%team,%client) //keep lives constant between team switches
{
    if(%mini.lives > 0 && %client.tempLives >= 0)
    {
        %client.setLives(%client.tempLives);
        %client.tempLives = "";
    }
}

function Slayer_INF_Teams_onLeave(%mini,%team,%client) //call function for when the client has left the team
{
    schedule(10,0,Slayer_INF_postLeave,%mini,%team,%client);
}

function Slayer_INF_postLeave(%mini,%team,%client) //check if a team is empty, if so, end round
{
    if(%team.numMembers == 0 || !%team.getLiving())
    {
        %teams = %mini.Teams;
        %teamCount = %teams.getCount();
        if(%teamCount == 2) //All but one team && A team becomes empty
            %mini.endRound((((%a = %mini.Teams.getObject(0)) == %team) ? %mini.Teams.getObject(1) : %a));
        else
        {
            for(%i=0;%i<%teamCount;%i++) //check teams for life and membership
            {
                %a = %teams.getObject(0);
                if(!%a.numMembers || !%a.getLiving())
                    continue;
                if(%teamList $= "")
                    %teamList = %a;
                else
                    %teamList = %teamList TAB %a;
                %count++;
            }
            if(%count == 1 && !%mini.endRoundOnEmptyTeam)
                %mini.endRound(%teamList);
            else if(%mini.endRoundOnEmptyTeam)
                %mini.endRound(%teamList);
        }
    }
}

function Slayer_INF_onModeStart(%mini)
{
    %mini.Teams.balanceTeams = 0;
}

function Slayer_INF_preReset(%mini,%client)
{
    %mini.Teams.balanceTeams = 1;
    %mini.Teams.shuffleTeams(1);
    %mini.Teams.balanceTeams = 0;
}
