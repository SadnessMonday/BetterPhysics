# BetterPhysics
[![Unity 6000.0+](https://img.shields.io/badge/unity-6000.0%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/dbrizov/NaughtyAttributes/blob/master/LICENSE)

BetterPhysics improves the functionality of Rigidbody Physics in Unity. Bring complex physics systems to life quickly without messing around with complicated physics code.


Selective Kinematics lets your objects behave like realistic dynamic bodies while selectively completely ignoring objects of your choice. For example imagine easily designing a kinematic character controller that can still be freely swatted around by a big boss! Selective Kinematics takes advantage of Unity's Physics [ContactModifyEvent](https://docs.unity3d.com/ScriptReference/Physics.ContactModifyEvent.html) API.


Configurable Rigidbody Speed Limits allow you to enforce flexible limits to your body's velocity without the need to write complicated drag code. Freely use AddForce without worrying about the object accelerating forever. Configure limits in global axes, local axes, or omnidirectionally!

## System Requirements
Unity **2022.2** or later versions. Don't forget to include the SadnessMonday.BetterPhysics namespace.

## Installation
1. The package is available on the [openupm registry](https://openupm.com). You can install it via [openupm-cli](https://github.com/openupm/openupm-cli).
```
openupm add com.dbrizov.naughtyattributes
```
1. You can also install via git url by adding this entry in your **manifest.json**
```
"com.sadnessmonday.betterphysics": "https://github.com/SadnessMonday/BetterPhysics.git?path=/Assets/BetterPhysics"
```
2. You can also download it from the [Asset Store]([https://assetstore.unity.com/packages/tools/utilities/naughtyattributes-129996](https://assetstore.unity.com/packages/tools/physics/betterphysics-selective-kinematics-244370))

## Documentation
- [Documentation](https://sadnessmonday.com/pages/betterphysics/)

## Support
BetterPhysics is an open-source project that I am developing in my free time. If you like it you can support me by donating.

- [ko-fi]([https://paypal.me/dbrizov](https://ko-fi.com/praetorblue))
