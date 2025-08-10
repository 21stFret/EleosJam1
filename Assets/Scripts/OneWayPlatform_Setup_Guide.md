# One-Way Platform Setup Guide

## The Problem You Had
Your old IEnumerator system was turning off collision for a fixed time, which didn't work well because:
- Players could get stuck in platforms
- Timing was inconsistent with different fall speeds
- It didn't properly handle the "one-way" nature (collide from above, pass through from below)

## The New Solution

### Method 1: Using Unity's PlatformEffector2D (Recommended)

1. **Set up your platform GameObject:**
   - Add a `Collider2D` (Box Collider 2D or other)
   - Add a `PlatformEffector2D` component
   - Add the `OneWayPlatform` script I created

2. **Configure the PlatformEffector2D:**
   - ✅ **Use One Way**: Checked
   - ✅ **Used By Effector**: Checked (on the Collider2D)
   - **Surface Arc**: 180 (collision from above only)
   - **Side Arc**: 0 (no side collision)

3. **Layer Setup:**
   - Put your platforms on a specific layer (e.g., "OneWayPlatforms")
   - Set the `oneWayPlatformMask` in your ExorcistController to match this layer

### Method 2: Custom Physics-Based Solution

If you prefer more control, you can create a custom script that checks the player's position relative to the platform:

```csharp
public class CustomOneWayPlatform : MonoBehaviour
{
    private Collider2D platformCollider;
    
    void Start()
    {
        platformCollider = GetComponent<Collider2D>();
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if player is above the platform
            bool playerAbove = other.transform.position.y > transform.position.y;
            
            // Enable/disable collision based on position
            Physics2D.IgnoreCollision(platformCollider, other, !playerAbove);
        }
    }
}
```

## How It Works Now

1. **Normal Collision**: When player approaches from above, they land on the platform
2. **Jump Through**: When player presses jump + down while on platform, the effector is temporarily disabled
3. **Pass Through from Below**: Player can jump up through platforms naturally
4. **Automatic Re-enable**: Collision is restored after the player clears the platform

## Benefits of This Approach

- ✅ **Automatic collision detection** based on player position
- ✅ **No timing issues** - collision is restored when appropriate
- ✅ **Consistent behavior** regardless of fall speed
- ✅ **Works with Unity's physics system** naturally
- ✅ **Easy to set up** on existing platforms

## Testing Your Setup

1. Player should land on platforms when jumping from above
2. Player should pass through when jumping from below
3. Player should drop through when pressing Jump + Down while standing on platform
4. No getting stuck in platforms

## Troubleshooting

**Player falls through platforms they should land on:**
- Check that Surface Arc is 180
- Make sure Used By Effector is checked
- Verify the platform is on the correct layer

**Player can't drop through platforms:**
- Check that your input system is detecting vertical input correctly
- Verify the oneWayPlatformMask matches your platform layer
- Make sure the DropThroughPlatform method is being called

**Player gets stuck in platforms:**
- Increase the jumpDownDuration slightly
- Check that effectors are being re-enabled properly
- Verify platform collider sizes aren't too large

Let me know if you need help setting up any of these components!
