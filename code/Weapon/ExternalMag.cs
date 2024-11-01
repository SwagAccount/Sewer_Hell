using System;
using Sandbox;
namespace trollface;
public sealed class ExternalMagazine : MagazineBase
{
	[Property] public GameObject EjectPoint {get;set;}
	[Property] public TextRenderer AmmoCount {get;set;}

    [Property] public bool CantEject {get;set;}
    ChunkDealer chunkDealer;
	protected override void OnStart()
	{
		chunkDealer = Scene.Components.GetInChildren<ChunkDealer>();
	}
	bool triggerPressed;
    bool triggerWasPressed;
	protected override void OnFixedUpdate()
	{
        
        base.OnFixedUpdate();
        if(AmmoCount != null) AmmoCount.Text = Contents.Count.ToString();
        if(CantEject) return;
		if(!item.mainHeld) return;

        triggerWasPressed = triggerPressed;
        triggerPressed = item.Controller.Trigger > 0.75f;
        
        if(triggerPressed && !triggerWasPressed) TriggerPress();
        
        if(!triggerPressed && triggerWasPressed) TriggerRelease();
        
	}

    void TriggerPress()
    {
        if(EjectPoint.Children.Count > 0) {TriggerRelease(); return;} // I don't fucking know, could cause problems if not done (it is all in my head)
        if(Contents.Count <= 0) return;
        GameObject newItem = GetPrefab(0).Clone();
        newItem.SetParent(EjectPoint);
        newItem.Transform.LocalPosition = Vector3.Zero;
        newItem.Transform.LocalRotation = Rotation.Identity;
        newItem.Components.Get<Rigidbody>().MotionEnabled = false;
        Contents.RemoveAt(0);
        UpdateVisuals();
    }

    void TriggerRelease()
    {
        if(EjectPoint.Children.Count <= 0) return;
        EjectPoint.Children[0].Components.Get<Rigidbody>().MotionEnabled = true;
        chunkDealer.PlaceInChunk(EjectPoint.Children[0]);
    }
}
