# SimpleSample
Adds the Graph Window to the Unity Editor

This is a Work In Progress and is not guaranteed to not break.
I am buiding it in Unity 5.6.0. I have sort-of tried to make it work with earlier versions, but not really.

## TO USE

Add the VelocitySampler to objects in the scene. Select them while the scene is playing and open the Graph Window (Window->Graph Window).

* Sampled data is automatically deleted when the scene is restarted
* Sampled data is automatically deleted when the scene is reloaded
* Sampled data will persist after Pausing or Stopping play

### VECTORSAMPLER PUBLIC VARIABLES
* Transform: The transform you want to track. Obviously.
* Color: The color of the line in the Graph Window
* Sample Period: Target amount of time between individual samples
* Num Samples: Maximum number of samples to save
