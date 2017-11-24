using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class CustomDialogueRunner : Yarn.Unity.DialogueRunner
{
	public int seed = 0;

	private static string YarnHack(Match match)
	{
		return Regex.Replace(match.Value, @"(?<!---|\]\])\n(?!===)", "<br>");
	}

	private static string PageNumberReplacer(Match match)
	{
		string original = match.Value;
		string pageTurn = match.Groups[1].Value;
		string choice = match.Groups[2].Value;
		int pageNumber;
		if(!pageNumbers.TryGetValue(choice, out pageNumber)) {
			throw new KeyNotFoundException("Couldn't get page number for choice!");
		}
		return original.Replace(pageTurn, "urn to page " + pageNumber.ToString());
	}

	public static Dictionary<string, int> pageNumbers = new Dictionary<string, int>();
	public string Paginate(string s)
	{
		Random.InitState(seed);
		MatchCollection m = Regex.Matches(s, @"\[\[.*?\|(.*?)\]\]");

		// generate sequential numbers with gaps
		int[] numbers = new int[m.Count];
		int curPage = 5; // only generate 5 and up
		for(int i = 0; i < m.Count; ++i) {
			numbers[i] = curPage;
			curPage += Random.Range(3, 16);
			curPage += 1 - curPage % 2;
		}

		// shuffle numbers
		for(int i = 0; i < m.Count; ++i) {
			int tmp = numbers[i];
			int r = Random.Range(i, numbers.Length);
			numbers[i] = numbers[r];
			numbers[r] = tmp;
		}

		// assign choices to pages
		for(int i = 0; i < m.Count; ++i) {
			string choice = m[i].Groups[1].Value;
			if(!pageNumbers.ContainsKey(choice)) {
				pageNumbers.Add(choice, numbers[i]);
			}
		}
		pageNumbers["Start"] = 1; // force start to 1
		return Regex.Replace(s, @"\[\[.*?(urn to page #).*?\|(.*?)\]\]", new MatchEvaluator(PageNumberReplacer));
	}

	/// Start the dialogue
	void Start()
	{
		// Ensure that we have our Implementation object
		if(dialogueUI == null) {
			Debug.LogError("Implementation was not set! Can't run the dialogue!");
			return;
		}

		// And that we have our variable storage object
		if(variableStorage == null) {
			Debug.LogError("Variable storage was not set! Can't run the dialogue!");
			return;
		}

		// Ensure that the variable storage has the right stuff in it
		variableStorage.ResetToDefaults();

		// Load all scripts
		if(sourceText != null) {
			foreach(var source in sourceText) {
				// load and compile the text
				// THIS IS THE HACKED BIT
				dialogue.LoadString(Regex.Replace(Paginate(source.text), @"(?s)---.*?\[\[|===", new MatchEvaluator(YarnHack)), source.name);
			}
		}

		if(startAutomatically) {
			StartDialogue();
		}

		if(stringGroups != null) {
			// Load the string table for this language, if appropriate
			var stringsGroup = new List<Yarn.Unity.LocalisedStringGroup>(stringGroups).Find(
				entry => entry.language == (shouldOverrideLanguage ? overrideLanguage : Application.systemLanguage)
			);

			if(stringsGroup != null) {
				foreach(var table in stringsGroup.stringFiles) {
					this.AddStringTable(table.text);
				}
			}
		}

	}
}
