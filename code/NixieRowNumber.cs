using Sandbox;
using System;

public sealed class NixieRowNumber : Component
{
	[Property] public int TestNumber {get;set;}

	[Button("TestNumber")] public void TestSetNumber() => SetNumber(TestNumber);
	List<NixieTubeNumber> nixieTubeNumbers;
	protected override void OnStart()
	{
		nixieTubeNumbers = new List<NixieTubeNumber>();
		foreach(GameObject c in GameObject.Children)
		{
			nixieTubeNumbers.Add(c.Components.Get<NixieTubeNumber>());
		}
	}
	public async void SetNumber(int number)
	{
		char[] numberChars = number.ToString().ToCharArray();
		Array.Reverse(numberChars);

		Log.Info(numberChars[0] - '0');
		for(int i = 0; i < nixieTubeNumbers.Count; i++)
		{
			if(i < numberChars.Length && number > -1)
				await nixieTubeNumbers[i].SetNumber(numberChars[i] - '0');
			else
				await nixieTubeNumbers[i].SetNumber(-1);
		}
	}
}
