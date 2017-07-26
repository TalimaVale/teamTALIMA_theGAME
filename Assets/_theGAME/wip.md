# Work In Progress

### Scene - Level 1
- [ ] Upgrade UI typography, to be sharp and clear
    - In Unity, fonts will be sharper if set to a large font size and scaled down using the Transform component



### PlayerController.cs
- [ ] Comment current code



### CameraController.cs
- [ ] Comment current code
- [x] Take Interact layer objects off of camera's clipping check
- [ ] Fade out players who are close to camera
    - if a player has their camera zoomed close to another player's character, fade out that character
- [ ] Restructure code so that majority of UI checks are not in CameraController script
- [ ] Consider: Track MouseDown and MouseUp, so when we click down on UI and travel off of UI without releasing click, camera does not pan
    - This will make for better user experience



### Awesomeness.cs
- [ ] Comment current code



### MinigameCollectConsole.cs
- [ ] Comment current code



### MinigameCollectBlock.cs
- [ ] Comment current code
- [ ] BUGFIX: When player picks up block, material turns opaque
    - what part of our code/player prefab is turning the player's held item opaque?



### MousePointer.cs
- [ ] Make compatible with Mac and Linux OS
    - Currently uses a Windows library to successfully lock mouse in place when rotating camera
	- Either needs to be upgraded for general use, or needs additions to achieve functionality on Mac and Linux OS