# BetterPhysics
[![Unity 2022.2+](https://img.shields.io/badge/unity-2022.2%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/dbrizov/NaughtyAttributes/blob/master/LICENSE)

BetterPhysics improves the functionality of Rigidbody Physics in Unity. Bring complex physics systems to life quickly without messing around with complicated physics code.


Selective Kinematics lets your objects behave like realistic dynamic bodies while selectively completely ignoring objects of your choice. For example imagine easily designing a kinematic character controller that can still be freely swatted around by a big boss! Selective Kinematics takes advantage of Unity's Physics [ContactModifyEvent](https://docs.unity3d.com/ScriptReference/Physics.ContactModifyEvent.html) API.


Configurable Rigidbody Speed Limits allow you to enforce flexible limits to your body's velocity without the need to write complicated drag code. Freely use AddForce without worrying about the object accelerating forever. Configure limits in global axes, local axes, or omnidirectionally!

## System Requirements
Unity **2022.2** or later versions. Don't forget to include the SadnessMonday.BetterPhysics namespace.

## Installation
### 1. You can install directly from the Package Manager in Unity:

Open the package manager in Unity, press the + button at the top right, select "Add package from git URL" and use the following URL:
`https://github.com/SadnessMonday/BetterPhysics.git?path=/Assets/BetterPhysics`

### 2. You can also install by adding this entry in your **manifest.json**
```
"com.sadnessmonday.betterphysics": "https://github.com/SadnessMonday/BetterPhysics.git?path=/Assets/BetterPhysics"
```
### 3. You can also download it from the Unity Asset Store
[Asset Store Link](https://assetstore.unity.com/packages/tools/physics/betterphysics-selective-kinematics-244370)

## Documentation
- [Documentation](https://sadnessmonday.com/pages/betterphysics/)

## Community & Discussion

Want help with this asset? Want to just talk about it? Come join us on our [Discord Server](https://discord.gg/Nq2et6jzGt)

## Support
BetterPhysics is a *free* open-source project that I am developing in my free time. If you like it you can support me by donating:

- [ko-fi](https://ko-fi.com/praetorblue)
