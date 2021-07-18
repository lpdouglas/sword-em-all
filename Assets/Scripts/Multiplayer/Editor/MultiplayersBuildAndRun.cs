﻿using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public static class MultiplayersBuildAndRun {

	[MenuItem("Multiplayer/Replay")]
	static void PerformWin64Run (){
		string path = "Builds/Win64/" + GetProjectName() + ".exe";
		var proc = new Process {StartInfo = {FileName = Application.dataPath.Replace("Assets", "") + path}};
		proc.Start();
	}
	
	[MenuItem("Multiplayer/Run 1 Multiplayer")]
	static void PerformWin64Build1 (){
		PerformWin64Build (1);
	}

	[MenuItem("Multiplayer/Run 2 Multiplayers")]
	static void PerformWin64Build2 (){
		PerformWin64Build (2);
	}

	[MenuItem("Multiplayer/Run 3 Multiplayers")]
	static void PerformWin64Build3 (){
		PerformWin64Build (3);
	}

	[MenuItem("Multiplayer/Run 4 Multiplayers")]
	static void PerformWin64Build4 (){
		PerformWin64Build (4);
	}
	
	[MenuItem("Multiplayer/Run 5 Multiplayers")]
	static void PerformWin64Build5 (){
		PerformWin64Build (5);
	}
	[MenuItem("Multiplayer/Run 6 Multiplayers")]
	static void PerformWin64Build6 (){
		PerformWin64Build (6);
	}

	static void PerformWin64Build (int playerCount) {
		//EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);

		string path = "Builds/Win64/" + GetProjectName() + ".exe";
		
		BuildPipeline.BuildPlayer (GetScenePaths (), path, BuildTarget.StandaloneWindows64, BuildOptions.None);
		
		for (int i = 0; i < playerCount; i++) {
			var proc = new Process {StartInfo = {FileName = Application.dataPath.Replace("Assets", "") + path}};
			proc.Start();
		}
	}

	static string GetProjectName() {
		string[] s = Application.dataPath.Split('/');
		return s[s.Length - 2];
	}

	static string[] GetScenePaths() {
		string[] scenes = new string[EditorBuildSettings.scenes.Length];

		for(int i = 0; i < scenes.Length; i++) {
			scenes[i] = EditorBuildSettings.scenes[i].path;
		}

		return scenes;
	}

}