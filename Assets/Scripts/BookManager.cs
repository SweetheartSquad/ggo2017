using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookManager : Yarn.Unity.DialogueUIBehaviour
{
	public GameObject bookButton;
	public TextMeshProUGUI description;
	public TextMeshProUGUI descriptionContinued;
	public TextMeshProUGUI pageNumberL;
	public TextMeshProUGUI pageNumberR;
	public GameObject choices;
	public GameObject book;
	public Material pageMat;
	private int pageMatColorIdx;

	private Yarn.OptionChooser SetSelectedOption;

	private static string lineIndent = "<space=5em>";
	private static float spriteSize = 200;
	int page = 0;

	private void Start()
	{
		description.text = "";
		pageNumberL.text = "1";
		pageNumberR.text = "2";
		pageMatColorIdx = Shader.PropertyToID("_TextColor");
	}

	public override IEnumerator NodeComplete(string nextNode)
	{
		if(nextNode != null) {
			pageNumberL.text = CustomDialogueRunner.pageNumbers[nextNode].ToString();
			pageNumberR.text = (CustomDialogueRunner.pageNumbers[nextNode] + 1).ToString();
		}

		// make text visible
		Color c = pageMat.GetColor(pageMatColorIdx);
		c.a = 1.0f;
		pageMat.SetColor(pageMatColorIdx, c);

		return base.NodeComplete(nextNode);
	}

	// Display the page text
	public override IEnumerator RunLine(Yarn.Line line)
	{
		string s = line.text;
		s = lineIndent + Regex.Replace(s, @"<br>(?=\w)", "<br>" + lineIndent);
		s = s.Replace("END", "<br><align=\"center\"><b>The End</b></align>");
		s = s.Replace("<b><i>", "<font=\"BenguiatStd-BoldItalic SDF\">");
		s = s.Replace("<i><b>", "<font=\"BenguiatStd-BoldItalic SDF\">");
		s = s.Replace("</i></b>", "</font>");
		s = s.Replace("</b></i>", "</font>");

		s = s.Replace("<b>", "<font=\"BenguiatStd-Bold SDF\">");
		s = s.Replace("</b>", "</font>");

		s = s.Replace("<i>", "<font=\"BenguiatStd-BookItalic SDF\">");
		s = s.Replace("</i>", "</font>");

		s = Regex.Replace(s, "<sprite.*?>", new MatchEvaluator(Figure));
		while(s.Substring(s.Length - 4, 4) == "<br>") {
			s = s.Substring(0, s.Length - 4);
		}
		description.text = s;


		// dumb fake linking based on line breaks
		s = "";
		string[] stringSeparators = new string[] { "<br>" };
		string[] ss = description.text.Split(stringSeparators, System.StringSplitOptions.RemoveEmptyEntries);

		int t = ss.Length;
		description.ForceMeshUpdate();
		do {
			description.text = string.Join("<br>", ss, 0, t);
			description.ForceMeshUpdate();
			t -= 1;
		} while(description.textInfo.lineCount > 25 && t >= 0);
		descriptionContinued.text = string.Join("<br>", ss, t+1, ss.Length - t - 1);

		yield return null;
	}

	/// Show a list of options and wait for input
	public override IEnumerator RunOptions(Yarn.Options optionsCollection, Yarn.OptionChooser optionChooser)
	{
		if(optionsCollection.options.Count > 0) {
			choices.SetActive(true);
		}
		// Display each option in a button, and make it visible
		GameObject[] buttons = new GameObject[optionsCollection.options.Count];
		for(int i = 0; i < optionsCollection.options.Count; ++i) {
			GameObject choice = Instantiate(bookButton);
			buttons[i] = choice;
			choice.GetComponent<TextMeshProUGUI>().text = optionsCollection.options[i];
			choice.transform.SetParent(choices.transform, false);
			int o = i;
			int p = int.Parse(Regex.Match(optionsCollection.options[i], @"urn to page (\d*)").Groups[1].Value);
			choice.GetComponent<Button>().onClick.AddListener(() => SetOption(o, p));
		}

		SetSelectedOption = optionChooser;

		// Wait until the chooser has been used and then removed (see SetOption below)
		while(SetSelectedOption != null) {
			yield return null;
		}

		// disable interaction
		foreach(GameObject go in buttons) {
			go.GetComponent<TextMeshProUGUI>().raycastTarget = false;
		}

		// start flips
		bool flipDir = int.Parse(pageNumberL.text) < page;
		int numFlips = Mathf.Clamp(Mathf.Abs(int.Parse(pageNumberL.text) - page) / 2, 6, 24);
		float speed = 12.0f / numFlips;
		for(int i = 0; i < numFlips; ++i) {
			GetComponent<PageFlip>().Flip(flipDir, 0.07f * i * speed + Random.Range(0.0f, 0.03f), i == 0 || i == numFlips - 1);
		}
		GetComponent<AudioSource>().pitch = Mathf.Clamp(1.0f / (speed), 0.75f, 1.25f) + Random.Range(-.1f, 0.1f);
		GetComponent<AudioSource>().Play();

		// wait until flips are partially done
		yield return new WaitForSeconds(1.2f);

		// fade out text
		Color c = pageMat.GetColor(pageMatColorIdx);
		for(int i = 1; i <= 15; ++i) {
			c.a = 1.0f - i / 15.0f;
			pageMat.SetColor(pageMatColorIdx, c);
			yield return new WaitForEndOfFrame();
		}

		// clear old page
		description.text = "";
		descriptionContinued.text = "";
		pageNumberL.text = "";
		pageNumberR.text = "";

		// Destroy buttons, hide divider
		while(choices.transform.childCount > 1) {
			Transform lastChoice = choices.transform.GetChild(choices.transform.childCount - 1);
			lastChoice.SetParent(null);
			Destroy(lastChoice.gameObject);
		}
		choices.SetActive(false);
	}

	/// Called by buttons to make a selection.
	public void SetOption(int _selectedOption, int _page)
	{
		page = _page;
		// Call the delegate to tell the dialogue system that we've
		// selected an option.
		SetSelectedOption(_selectedOption);

		// Now remove the delegate so that the loop in RunOptions will exit
		SetSelectedOption = null;
	}

	/// Run an internal command.
	public override IEnumerator RunCommand(Yarn.Command command)
	{
		// "Perform" the command
		Debug.Log("Command: " + command.text);

		yield break;
	}

	/// Called when the dialogue system has started running.
	public override IEnumerator DialogueStarted()
	{
		Debug.Log("Dialogue starts");
		description.text = "";
		descriptionContinued.text = "";

		yield break;
	}

	/// Called when the dialogue system has finished running.
	public override IEnumerator DialogueComplete()
	{
		Debug.Log("Dialogue ends");

		yield break;
	}



	// surrounds with linebreaks, embiggens, and centers
	public static string Figure(Match match)
	{
		string s = "<align=\"center\"><br><size=" + spriteSize + ">" + match.Value + "</size><br></align>";
		return s;
	}
}