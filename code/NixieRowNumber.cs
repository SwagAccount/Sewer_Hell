using Sandbox;
using System;

public sealed class NixieRowNumber : Component
{
	[Property] public int TestNumber {get;set;}

	[Button("TestNumber")] public void TestSetNumber() => SetNumber(TestNumber);
	List<NixieTubeNumber> nixieTubeNumbers;
	protected override void OnStart()
	{
		GetNumbers();
	}
	void GetNumbers()
	{
		nixieTubeNumbers = new List<NixieTubeNumber>();
		foreach(GameObject c in GameObject.Children)
		{
			nixieTubeNumbers.Add(c.Components.Get<NixieTubeNumber>());
		}
	}
	public async void SetNumber(int number)
	{
		if(nixieTubeNumbers == null)
			GetNumbers();
		char[] numberChars = number.ToString().ToCharArray();
		Array.Reverse(numberChars);
		for(int i = 0; i < nixieTubeNumbers.Count; i++)
		{
			if(i < numberChars.Length && number > -1)
				await nixieTubeNumbers[i].SetNumber(numberChars[i] - '0');
			else
				await nixieTubeNumbers[i].SetNumber(-1);
		}
	}
}
