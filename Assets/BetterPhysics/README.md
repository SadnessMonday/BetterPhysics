
# Better Physics
### Introduction

Welcome to BetterPhysics by Sadness Monday Productions!

We're here to help you take your physics-enabled games to the next level

## BetterRigidbody
The primary point of interaction for BetterPhysics is the BetterRigidbody component. Add a BetterRigidbody component to any existing Rigidbody GameObject to enhance its capabilities!

### Speed Limits

BetterRigidbody allows you to add customizable speed limits to your Rigidbodies. 

<sup style="display: inline-block;">**tip:** Limits can be mixed and matched as much as you'd like, and they come with multiple modes</sup>

#### Limit Type

##### Soft

Soft limits are limits only on the velocity your object will reach when you use AddForce to change its velocity. Soft limits still allow outside forces to affect your body naturally. For example you could create a space ship that has a 100 m/s soft speed limit, but if your ship gets hit by a fast moving asteroid it can still fly away with unlimited velocity!

##### Hard Limits

Hard limits are inflexible speed limits. Think of them like an improvement on the built-in Constraints for Rigidbodies. But instead of limiting velocities to 0, hard speed limits allow movement, up to a point you configure.

#### Directionality

Directionality controls how the speed limit is applied in space

##### Omnidirectional

Omnidirectional limits  apply equally in all directions. You can think of an omnidirectional limit like a limit of the <b>magnitude</b> of the object's velocity.

##### World Axes

World axis limits apply to the Rigidbody's absolute velocity in world space. You can think of this as a limit on the built-in <b>velocity</b> property of the Rigidbody.

##### Local Axes

Local Axis limits apply to the velocity of the RIgidbody in its own coordinate space. This allows you to configure specific limits on the velocity that change depending on which direction the body is facing. For example you could set a limit for the forward velocity of the car, and have a different limit for the backwards or sideways velocity

<sup style="display: inline-block;">**tip:** World and Local axis limits can be Symmetrical or Asymmetrical per axis.</sup>

### Selective Kinematics
Each BetterRigidbody can be set up on a particular Physics Layer. You can set the Physics Layer from the Inspector for the BetterRigidbody component. See 

## BetterPhysics Settings

To access BetterPhysics settings go to **Edit -> Project Settings -> Better Physics**

From BetterPhysics settings page you can customize your physics layers by giving them names, adding new layers, and configuring the [Interactions Matrix](%5Blink%5D%28#The-Interactions-Matrix%29)

### The Interactions Matrix

The interactions matrix is similar to the Layer-Based collision matrix in the default physics settings window, with a twist. Each box determines how the interaction between the corresponding layers will work.

- When a given box is grey, the interactions between those layers will be normal
- When a given box is green, the layer on the left will act as if it is kinematic when interacting with the layer on the top.
- When a given box is yellow, the layer on the top will act as if it is kinematic when interacting with the layer on the left.