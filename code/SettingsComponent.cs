using System;
using Sandbox;
using Sandbox.Audio;
using Sandbox.UI;
using trollface;

public sealed class SettingsComponent : Component
{
	[Property] public float Volume {get;set;}
	[Property] public float Snap {get;set;}
	[Property] public float RotateSpeed {get;set;}
	[Property] public Lever SnapOn {get;set;}
	[Property] public NixieRowNumber VolumeDisplay {get;set;}
	[Property] public NixieRowNumber SnapDisplay {get;set;}
	[Property] public Slider SnapSlider {get;set;}
	[Property] public Slider VolumeSlider {get;set;}
	
	bool LastSnapOn;
	int lastSnap;
	int lastVolume;
	Mixer mixerGame;
	protected override void OnAwake()
	{
		mixerGame = Mixer.FindMixerByName("Game");
		Load();
	}
	public bool Updated()
	{
		int snapV = SnapSlider.GetValue();
		int volumeV = VolumeSlider.GetValue();

		bool updated =
			SnapOn.On != LastSnapOn ||
			lastSnap != snapV ||
			lastVolume != volumeV;

		return updated;
	}

	protected override void OnFixedUpdate()
	{
		int snapV = SnapSlider.GetValue();
		int volumeV = VolumeSlider.GetValue();
		if(SnapOn.On != LastSnapOn ||
			lastSnap != snapV ||
			lastVolume != volumeV)
		{
			Volume = GetVolume();
			Snap = SnapOn.On ? GetSnap() : Snap;
			RotateSpeed = !SnapOn.On ? GetRotateSpeed() : RotateSpeed;
			VolumeDisplay.SetNumber((int)MathF.Round(Volume*100));
			SnapDisplay.SetNumber(SnapOn.On ? (int)MathF.Round(Snap) : (int)MathF.Round(RotateSpeed));
			mixerGame.Volume = Volume;
			Save();
		}
		LastSnapOn = SnapOn.On;
		lastSnap = snapV;
		lastVolume = volumeV;
	}

	public float GetVolume()
	{
		return VolumeSlider.GetValue()/((float)VolumeSlider.Increments);
	}

	public float GetSnap()
	{
		return SnapSlider.GetValue() * 5;
	}

	public float GetRotateSpeed()
	{
		return SnapSlider.GetValue()*10;
	}

	public bool GetSnapOn()
	{
		return SnapOn.On;
	}

	void Save()
	{
		Settings settings = new Settings{
			Volume = Volume,
			Snap = Snap,
			RotateSpeed = RotateSpeed,
			SnapOn = GetSnapOn()
		};

		FileSystem.Data.WriteAllText("Settings.json", Json.Serialize(settings));
	}
	
	void Load()
	{
		Settings settings;
		if(FileSystem.Data.FileExists("Settings.json"))
			settings = Json.Deserialize<Settings>(FileSystem.Data.ReadAllText("Settings.json"));
		else
			settings = new Settings{
				Volume = 1,
				Snap = 30,
				RotateSpeed = 180,
				SnapOn = false
			};

		Volume = settings.Volume;
		
		Snap = settings.Snap;
		RotateSpeed = settings.RotateSpeed;
		SnapOn.On = settings.SnapOn;
		SnapOn.Rotater.Rotated.Transform.LocalRotation = new Angles(SnapOn.On ? SnapOn.Rotater.MaxAxis : SnapOn.Rotater.MinAxis);
		VolumeDisplay.SetNumber((int)MathF.Round(Volume*100));
		VolumeSlider.SetValue((int)MathF.Round(Volume * (VolumeSlider.Increments)));
		SnapDisplay.SetNumber(SnapOn.On ? (int)MathF.Round(Snap) : (int)MathF.Round(RotateSpeed));
		Log.Info((RotateSpeed/360) * SnapSlider.Increments);
		SnapSlider.SetValue((int)MathF.Round(SnapOn.On ? Snap/5 : (RotateSpeed/360) * SnapSlider.Increments));
		mixerGame.Volume = Volume;
		LastSnapOn = SnapOn.On;
		lastSnap = SnapSlider.GetValue();
		lastVolume = VolumeSlider.GetValue();
	}

	public class Settings
	{
		public float Volume {get;set;}
		public float Snap {get;set;}
		public float RotateSpeed {get;set;}
		public bool SnapOn {get;set;}
	}
}
