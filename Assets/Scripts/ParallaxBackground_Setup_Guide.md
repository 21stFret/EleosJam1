# Parallax Background Setup Guide

## Overview
This parallax background system creates a depth effect by moving background layers at different speeds relative to the camera movement. Closer layers move faster, distant layers move slower.

## Setup Instructions

### 1. Prepare Your Background Sprites
- Create multiple background layers (e.g., sky, mountains, trees, foreground)
- Each layer should be wider than your camera view for seamless scrolling
- Arrange layers in your scene from back to front

### 2. Set Up the Parallax System
1. **Create an empty GameObject** and name it "ParallaxManager"
2. **Add the `paralaxBackground` script** to this GameObject
3. **Assign your camera** to the Camera Transform field (or leave empty to auto-find main camera)

### 3. Configure Parallax Layers
For each background layer:
1. **Drag the SpriteRenderer** into the Parallax Layers array
2. **Set the Parallax Speed**:
   - `0.0` = No movement (static background)
   - `0.1-0.3` = Distant mountains/sky
   - `0.4-0.7` = Mid-distance objects
   - `0.8-1.0` = Near foreground objects
3. **Enable Infinite Scroll** for seamless looping
4. **Optional**: Enable Vertical Parallax for vertical camera movement

## Layer Configuration Examples

```
Layer 0: Sky/Clouds          - Speed: 0.1  - Infinite: Yes
Layer 1: Distant Mountains   - Speed: 0.2  - Infinite: Yes  
Layer 2: Hills               - Speed: 0.4  - Infinite: Yes
Layer 3: Trees               - Speed: 0.6  - Infinite: Yes
Layer 4: Foreground Props    - Speed: 0.8  - Infinite: Yes
```

## Settings Explained

### Global Settings
- **Follow Camera X**: Enable horizontal parallax movement
- **Follow Camera Y**: Enable vertical parallax movement  
- **Smooth Damping**: Smooths movement transitions (0 = instant, higher = smoother)

### Per-Layer Settings
- **Sprite Renderer**: The background sprite to apply parallax to
- **Parallax Speed**: How fast this layer moves (0-1, where 1 = same speed as camera)
- **Infinite Scroll**: Automatically tiles the sprite for endless scrolling
- **Vertical Parallax**: Enable vertical movement for this layer
- **Vertical Speed**: Speed of vertical parallax movement

## Tips for Best Results

### 1. Layer Organization
- Sort layers by depth (furthest to nearest)
- Use consistent naming (Background_0, Background_1, etc.)
- Group related sprites under empty GameObjects

### 2. Sprite Preparation
- Make backgrounds **2-3x wider** than camera view for smooth infinite scroll
- Use **seamless textures** that tile well
- Keep **similar art styles** across layers

### 3. Performance Optimization
- Use **sprite atlases** for multiple background elements
- **Disable unused features** (vertical parallax if not needed)
- **Limit the number of layers** (5-8 is usually sufficient)

### 4. Speed Guidelines
```
Sky/Atmosphere: 0.0 - 0.2
Distant terrain: 0.2 - 0.4  
Mid-distance: 0.4 - 0.6
Near objects: 0.6 - 0.8
Foreground: 0.8 - 1.0
```

## Common Issues & Solutions

### Problem: Layers not moving
- **Check**: Camera Transform is assigned
- **Check**: Parallax speeds are not set to 0
- **Check**: Follow Camera X/Y are enabled

### Problem: Stuttering movement
- **Solution**: Increase Smooth Damping value (try 0.1-0.3)
- **Solution**: Use LateUpdate instead of Update (already implemented)

### Problem: Gaps in infinite scroll
- **Solution**: Make sure sprites are wide enough
- **Solution**: Check sprite pivot is set to Center
- **Solution**: Ensure sprites don't have extra transparent padding

### Problem: Layers moving wrong direction
- **Solution**: Check camera is moving in expected direction
- **Solution**: Verify layer order (distant layers should have lower speeds)

## Runtime Control

You can control the parallax system at runtime:

```csharp
// Change layer speed
parallaxManager.SetParallaxSpeed(0, 0.3f);

// Toggle infinite scrolling
parallaxManager.ToggleInfiniteScroll(1, false);

// Reset all layers to starting positions
parallaxManager.ResetAllLayers();
```

## Advanced Features

### Vertical Parallax
- Enable for platformers with vertical camera movement
- Use lower speeds for subtle effect
- Great for tall backgrounds like skyscrapers or cliffs

### Dynamic Speed Changes
- Change parallax speeds during gameplay
- Create cinematic effects
- Respond to game events (speed boosts, slow motion, etc.)

### Multiple Parallax Managers
- Use different managers for different areas
- Enable/disable based on current scene
- Different settings for different environments

Enjoy creating your parallax backgrounds!
