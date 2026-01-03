# Maevn "Saint Drip" Private Penthouse - Documentation

## Overview
Maevn "Saint Drip" Private Penthouse is a 1/1 Ultra-Legendary NFT property located at the penthouse level of the Paris Hotel + Casino in OmniVegas.

## Location Details
- **City**: OmniVegas (Las Vegas, NV)
- **Building**: Paris Hotel + Casino
- **Floor**: 65 (Penthouse Level)
- **Position**: Top of Paris Hotel + Casino structure at Y-offset +180m

## Property Specifications

### Dimensions
- **Square Footage**: 15,000 sq ft
- **Width**: 100m
- **Length**: 150m  
- **Height**: 42m (3 floors)
- **Total Floors**: 3 stories

### Layout
- **Bedrooms**: 8
- **Bathrooms**: 10
- **Parking Spaces**: 8
- **Building Height Position**: 180m above ground level

## Features

### Luxury Amenities
- Three-Story Penthouse Design
- Private Elevator Access
- 360-Degree Las Vegas Strip Views
- Eiffel Tower Replica View (Paris Hotel landmark)
- Indoor Olympic-Sized Pool
- Private Casino Suite
- Full Service Bar
- Chef's Kitchen
- Wine Cellar
- Theater Room
- Spa & Sauna
- Rooftop Garden
- Helipad Access
- Security Room
- Smart Home Integration

### Views
- Las Vegas Strip
- Paris Hotel Eiffel Tower Replica
- Bellagio Fountains
- Mountain Range

### Services
- 24/7 Concierge
- Private Chef Available
- Butler Service
- Personal Security
- Valet Parking
- Housekeeping

## NFT Metadata
- **Rarity**: Ultra-Legendary
- **Edition Size**: 1/1 (One of One)
- **Token Type**: ERC-721
- **Purchase Price**: 500,000 $OMNI
- **Monthly Rent**: N/A (Owner-occupied only)

## Special Benefits
- **Casino Revenue Share**: 0.5% of Paris Hotel + Casino revenue
- **Reputation Bonus**: +100 OmniVegas reputation upon ownership
- **VIP Access**: Exclusive access to all Paris Hotel + Casino events
- **Reserved Parking**: Dedicated parking in Paris Hotel + Casino garage

## Technical Implementation

### Parent Structure: Paris Hotel + Casino
```
Building: Paris Hotel + Casino
- Position: landmarkPosition (Vector3.zero base)
- Height: 180m
- Width: 120m
- Depth: 100m
- Style: Neon
- Value: 300,000 $OMNI
```

### Penthouse Structure: Maevn "Saint Drip" Private Penthouse
```
Building: Maevn "Saint Drip" Private Penthouse
- Position: landmarkPosition + Vector3(0, 180f, 0) [penthouse level]
- Height: 42m
- Width: 100m
- Depth: 150m
- Style: Neon
- Value: 500,000 $OMNI
```

### Code Reference
File: `/Assets/Scripts/AI/ProceduralGeneration.cs`

```csharp
case "OmniVegas":
    // Paris Hotel + Casino - Main landmark building
    GenerateLandmark("Paris Hotel + Casino", landmarkPosition, 
        BuildingStyle.Neon, 300000f, 180f, 120f, 100f);
    
    // Maevn "Saint Drip" Private Penthouse - At penthouse level (floor 65)
    // Positioned at the top of the Paris Hotel + Casino structure
    GenerateLandmark("Maevn 'Saint Drip' Private Penthouse", 
        landmarkPosition + new Vector3(0, 180f, 0), 
        BuildingStyle.Neon, 500000f, 42f, 100f, 150f);
    break;
```

### Prefab Reference
File: `/Assets/Prefabs/Housing/Penthouses/MaevnSaintDripPrivatePenthouse.json`

## Architecture Notes
- The penthouse sits atop the Paris Hotel + Casino, with its floor starting at 180m elevation
- The Y-offset of +180m ensures the penthouse is positioned at the top of the hotel structure
- The 42m height represents the 3-story penthouse spanning from floor 65 to the rooftop
- The structure mirrors real-world Paris Las Vegas casino architecture with a modern neon aesthetic

## Ownership Benefits in Game
1. **Economic**: 0.5% revenue share from all Paris Hotel + Casino gambling, dining, and event income
2. **Social**: +100 reputation boost in OmniVegas zone
3. **Access**: VIP entry to all casino events, shows, and exclusive areas
4. **Utility**: Premium spawn point with helipad access for rapid travel

## Related Properties
- **Maevn Mansion**: Off-Strip Estate (separate property, 1/1 Ultra-Legendary NFT, 1,000,000 $OMNI)

## See Also
- [OmniVegas City Infrastructure](CITY_INFRASTRUCTURE.md)
- [Procedural Generation Documentation](PROCEDURAL_GENERATION.md)
- [Property Ownership System](../Assets/Scripts/Modular/PropertyOwnershipSystem.cs)
