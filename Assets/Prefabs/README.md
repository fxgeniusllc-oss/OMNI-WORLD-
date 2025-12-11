# OmniWorld Prefabs

This directory contains reusable prefabs for the OmniWorld game.

## Structure

```
Prefabs/
├── Housing/
│   ├── Apartments/
│   ├── Condos/
│   ├── Mansions/
│   └── Penthouses/
├── Vehicles/
│   ├── Cars/
│   ├── Bikes/
│   └── Aircraft/
└── Avatars/
    ├── Base/
    ├── Customization/
    └── Animations/
```

## Housing Prefabs

### Residential Properties
- Studio Apartment
- 1-Bedroom Apartment
- 2-Bedroom Condo
- Luxury Penthouse
- Suburban House
- Mansion

Each housing prefab should include:
- Colliders for physics
- Interior/exterior models
- Door interaction points
- Ownership data component
- NFT metadata component

## Vehicle Prefabs

### Ground Vehicles
- Compact Car
- Sports Car
- Luxury Sedan
- SUV
- Motorcycle

### Air Vehicles
- Helicopter
- Private Jet

Each vehicle prefab should include:
- Rigidbody for physics
- Vehicle controller script
- Audio sources
- Particle effects (exhaust, etc.)

## Avatar Prefabs

### Base Character
- Humanoid rig (compatible with Unity's Mecanim)
- Animator controller
- Character controller
- IK targets for animations

### Customization
- Hair styles
- Clothing items
- Accessories
- Facial features

## Creating New Prefabs

1. Create the 3D model or import from Asset Store
2. Add required components
3. Configure settings
4. Save as prefab in appropriate folder
5. Tag appropriately
6. Test in scene

## Naming Convention

Use the format: `Category_Name_Variant`

Examples:
- `Housing_Apartment_Studio`
- `Vehicle_Car_Sports`
- `Avatar_Base_Male`

## Optimization

- Use LOD groups for complex models
- Optimize polygon count
- Use texture atlases where possible
- Implement object pooling for frequently spawned prefabs

## Notes

- All prefabs must have a unique identifier
- Include metadata for blockchain integration
- Ensure collision layers are correct
- Test prefabs in all target platforms
