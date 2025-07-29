# ROI (Region of Interest) Encoding in CoreJ2K

## Overview

Region of Interest (ROI) encoding is a powerful feature in JPEG2000 that allows you to specify areas of an image that should receive higher priority during compression. This ensures that important regions maintain better quality, especially at lower bitrates.

CoreJ2K implements ROI using the **Maxshift method**, which scales wavelet coefficients in the background (non-ROI) regions. This allows the decoder to identify ROI coefficients by their magnitude alone, without requiring a separate ROI mask.

## Benefits of ROI Encoding

- **Quality preservation**: Critical image areas maintain higher quality
- **Bandwidth efficiency**: Allocate more bits to important regions
- **Progressive transmission**: ROI data can be transmitted first
- **Flexible shapes**: Support for rectangular, circular, and arbitrary regions

## ROI Parameters

### Rroi - Define ROI Shape and Location

The main parameter for specifying ROIs. Supports three shape types:

**Syntax:**
```
[<component idx>] R <left> <top> <width> <height>     // Rectangular
[<component idx>] C <center_x> <center_y> <radius>    // Circular  
[<component idx>] A <filename>                         // Arbitrary (PGM mask)
```

- **Component index** (optional): Specifies which color components contain the ROI
  - Omit to apply to all components
  - Use comma-separated values for multiple components: `0,2`
  - Use hyphen for ranges: `0-2`
- **Coordinates**: All values are in pixels relative to the image origin (0,0)
- **Multiple ROIs**: Each R, C, or A starts a new ROI definition

### Ralign - Block-Aligned ROI Mode

**Values:** `on` | `off` (default: `off`)

When enabled, ROI masks are limited to covering entire code-blocks. This mode:
- Improves encoding efficiency
- Avoids coefficient scaling by using distortion scaling instead
- May include slightly more area than specified

### Rstart_level - Resolution Levels in ROI

**Values:** `<level>` (default: `-1`)

Forces the lowest resolution levels to belong to the ROI:
- `0`: Lowest resolution level belongs to ROI
- `1`: Two lowest resolution levels belong to ROI
- `-1`: Disabled (default)

This ensures ROI information is available early in progressive transmission.

### Rno_rect - Disable Fast Rectangle Processing

**Values:** `on` | `off` (default: `off`)

When enabled, forces generic ROI mask generation even for rectangular ROIs. Useful for testing or when precise rectangular boundaries are critical.

## Examples

### Basic Rectangular ROI

```csharp
var parameters = new ParameterList();
parameters["Rroi"] = "R 100 50 300 200";  // x=100, y=50, width=300, height=200
parameters["rate"] = "0.5";               // 0.5 bits per pixel

byte[] encoded = J2kImage.ToBytes(image, parameters);
```

### Multiple ROIs

```csharp
var parameters = new ParameterList();
// Define two rectangular ROIs
parameters["Rroi"] = "R 50 50 100 100 R 200 150 150 100";
parameters["rate"] = "1.0";

byte[] encoded = J2kImage.ToBytes(image, parameters);
```

### Circular ROI

```csharp
var parameters = new ParameterList();
// Circle centered at (256, 256) with radius 128
parameters["Rroi"] = "C 256 256 128";
parameters["rate"] = "0.75";

byte[] encoded = J2kImage.ToBytes(image, parameters);
```

### Component-Specific ROI

```csharp
var parameters = new ParameterList();
// Apply ROI only to components 0 and 1 (e.g., luminance and chrominance)
parameters["Rroi"] = "0,1 R 100 100 200 200";
parameters["rate"] = "1.0";

byte[] encoded = J2kImage.ToBytes(image, parameters);
```

### Arbitrary Shape ROI Using PGM Mask

```csharp
var parameters = new ParameterList();
// Use a PGM file as ROI mask (non-zero pixels indicate ROI)
parameters["Rroi"] = "A roi_mask.pgm";
parameters["rate"] = "1.0";

byte[] encoded = J2kImage.ToBytes(image, parameters);
```

The PGM mask file must:
- Have the same dimensions as the source image
- Use non-zero pixel values to indicate ROI regions
- Use zero values for background regions

### Block-Aligned ROI for Better Performance

```csharp
var parameters = new ParameterList();
parameters["Rroi"] = "R 64 64 256 256";
parameters["Ralign"] = "on";     // Align to code-block boundaries
parameters["rate"] = "0.5";

byte[] encoded = J2kImage.ToBytes(image, parameters);
```

### Including Low Resolution in ROI

```csharp
var parameters = new ParameterList();
parameters["Rroi"] = "R 100 100 400 300";
parameters["Rstart_level"] = "1";  // Include 2 lowest resolution levels
parameters["rate"] = "0.75";

byte[] encoded = J2kImage.ToBytes(image, parameters);
```

### Complex Multi-Component Example

```csharp
var parameters = new ParameterList();
// Different ROIs for different components
// Component 0: Large rectangular ROI
// Components 1,2: Smaller circular ROI
parameters["Rroi"] = "0 R 50 50 400 300 1,2 C 250 200 100";
parameters["Ralign"] = "on";
parameters["rate"] = "1.5";

byte[] encoded = J2kImage.ToBytes(image, parameters);
```

## Best Practices

### When to Use ROI

1. **Medical imaging**: Prioritize diagnostic regions
2. **Surveillance**: Focus on faces or movement areas  
3. **Document imaging**: Emphasize text regions
4. **Satellite imagery**: Highlight areas of interest

### Choosing ROI Parameters

- **Shape selection**:
  - Use rectangles for simple regions (fastest)
  - Use circles for radial regions of interest
  - Use arbitrary masks for complex shapes

- **Block alignment** (`Ralign`):
  - Enable for better compression efficiency
  - Disable when precise ROI boundaries are critical

- **Resolution levels** (`Rstart_level`):
  - Use when progressive transmission is important
  - Ensures ROI is visible even at low resolutions

### Performance Considerations

1. **Rectangular ROIs** are processed most efficiently
2. **Block-aligned mode** improves encoding speed
3. **Multiple small ROIs** may impact performance more than one large ROI
4. **Arbitrary masks** require additional memory and processing

## Technical Details

### Maxshift Method

CoreJ2K uses the Maxshift ROI method which:
1. Determines the maximum magnitude bit-plane (M) in the image
2. Scales down non-ROI coefficients by M bit-planes
3. Ensures ROI coefficients have higher magnitude than any background coefficient
4. Allows implicit ROI identification at the decoder without sending mask data

### ROI and Rate Control

When using ROI with rate control:
- ROI regions receive bit allocation priority
- Background quality degrades first when rate is limited
- Multiple ROIs are prioritized equally
- The `-rate` parameter still controls overall file size

### Limitations

- ROI information slightly increases file size due to additional signaling
- Very small ROIs relative to image size may not show significant benefit
- Arbitrary mask files must match source image dimensions exactly

## ROI for Facial Recognition Applications

Facial recognition systems often benefit from ROI encoding to prioritize critical facial features. CoreJ2K supports multi-region ROI encoding that works well with standard 68-point facial landmark detection models.

### 68-Point Facial Landmark Model

The 68-point model identifies key facial features:
- Points 0-16: Jawline
- Points 17-21: Right eyebrow
- Points 22-26: Left eyebrow
- Points 27-35: Nose
- Points 36-41: Right eye
- Points 42-47: Left eye
- Points 48-59: Outer mouth
- Points 60-67: Inner mouth

### Three-Region ROI Strategy

For efficient facial encoding, we recommend a 3-region approach:

#### Region 1: Eyes and Eyebrows (Highest Priority)
- Landmarks: Points 17-26 (eyebrows) and 36-47 (eyes)
- Contains critical identity information
- Recommended Maxshift: 3 bit-planes

#### Region 2: Nose, Mouth, and Jaw (Medium Priority)
- Landmarks: Points 0-16 (jaw), 27-35 (nose), 48-67 (mouth)
- Captures expression and structural features
- Recommended Maxshift: 2 bit-planes

#### Region 3: Full Face with Context (Lowest Priority)
- All 68 points plus 20% padding
- Includes background, hair, and ears
- Recommended Maxshift: 0 (standard compression)

### Implementation Example

```csharp
// Convert 68 facial landmark points to ROI regions
public static string GenerateFacialROI(Point[] landmarks)
{
    // Calculate bounding boxes for each region
    var eyesBox = CalculateBoundingBox(landmarks, 17, 26, 36, 47);
    var lowerFaceBox = CalculateBoundingBox(landmarks, 0, 16, 27, 35, 48, 67);
    var fullFaceBox = CalculateBoundingBox(landmarks, 0, 67, paddingRatio: 0.2);
    
    // Format as ROI parameters
    return string.Format("R {0} {1} {2} {3} R {4} {5} {6} {7} R {8} {9} {10} {11}",
        eyesBox.X, eyesBox.Y, eyesBox.Width, eyesBox.Height,
        lowerFaceBox.X, lowerFaceBox.Y, lowerFaceBox.Width, lowerFaceBox.Height,
        fullFaceBox.X, fullFaceBox.Y, fullFaceBox.Width, fullFaceBox.Height);
}

// Helper method to calculate bounding box from landmark indices
private static Rectangle CalculateBoundingBox(Point[] landmarks, 
    params int[] indices)
{
    var points = indices.SelectMany(GetIndexRange)
                       .Select(i => landmarks[i])
                       .ToList();
    
    int minX = points.Min(p => p.X);
    int minY = points.Min(p => p.Y);
    int maxX = points.Max(p => p.X);
    int maxY = points.Max(p => p.Y);
    
    // Add 10% padding by default
    int width = maxX - minX;
    int height = maxY - minY;
    int padX = width / 10;
    int padY = height / 10;
    
    return new Rectangle(
        minX - padX, minY - padY,
        width + 2 * padX, height + 2 * padY);
}
```

### Encoding with Facial ROI

```csharp
// Assuming you have 68 landmark points from detection
Point[] landmarks = DetectFacialLandmarks(image);

// Generate ROI specification
var parameters = new ParameterList();
parameters["Rroi"] = GenerateFacialROI(landmarks);
parameters["rate"] = "1.0";
parameters["Ralign"] = "on"; // Recommended for performance

// Encode with facial ROI
byte[] encoded = J2kImage.ToBytes(image, parameters);
```

### Best Practices for Facial ROI

1. **Landmark Detection Integration**:
   - Use established libraries (dlib, MediaPipe, OpenCV) for landmark detection
   - Validate landmark points before ROI generation
   - Handle edge cases where faces are partially visible

2. **Region Priority**:
   - Eyes region should always have highest priority
   - Adjust Maxshift values based on target file size
   - Consider using `Ralign=on` for better performance

3. **Multiple Faces**:
   - Process each face separately with its own set of ROIs
   - Limit total number of ROIs to maintain encoding efficiency
   - Prioritize primary face in group photos

4. **Quality vs. File Size**:
   - Higher Maxshift values preserve more detail but increase file size
   - Test different configurations for your specific use case
   - Consider progressive transmission requirements

## Multiple ROI Regions

When using multiple ROI regions, especially with overlapping areas, consider these guidelines:

### Overlapping Regions

CoreJ2K handles overlapping ROIs by applying the highest priority (Maxshift value) to overlapping areas. This means:
- Overlapping pixels receive the benefit of the highest applicable ROI
- No quality degradation occurs from overlap
- Order of ROI definition doesn't affect the result

### Performance Considerations

1. **Number of Regions**:
   - Each additional ROI adds processing overhead
   - Limit to 3-5 regions for optimal performance
   - Combine nearby small regions into larger ones when possible

2. **Region Size**:
   - Very small ROIs (< 32x32 pixels) may not show significant benefit
   - Large ROIs reduce overall compression efficiency
   - Aim for ROIs covering 10-40% of total image area

3. **Block Alignment**:
   - Enable `Ralign=on` for better performance with multiple regions
   - Slight boundary expansion is acceptable for most applications
   - Critical for real-time encoding scenarios

### Bit-Plane Shift Recommendations

For multi-priority ROI encoding:

| Priority Level | Recommended Shift | Use Case |
|----------------|------------------|----------|
| Critical       | 3-4 bit-planes   | Medical diagnosis, biometric features |
| High           | 2-3 bit-planes   | Important text, faces |
| Medium         | 1-2 bit-planes   | Secondary features |
| Low            | 0 bit-planes     | Background, context |

The actual Maxshift is calculated automatically, but these relative differences help achieve desired quality distribution.

## See Also

- [JPEG2000 Standard (ISO/IEC 15444-1)](https://www.iso.org/standard/78321.html)
- [CoreJ2K Main Documentation](README.md)