using System;
using System.IO;
using CoreJ2K;
using CoreJ2K.j2k.util;
using CoreJ2K.Util;

namespace CoreJ2K.Examples
{
    /// <summary>
    /// Examples demonstrating ROI (Region of Interest) encoding with CoreJ2K.
    /// These examples show how to use various ROI features to prioritize
    /// specific regions during JPEG2000 compression.
    /// </summary>
    public class ROIEncodingExamples
    {
        /// <summary>
        /// Basic rectangular ROI example.
        /// Demonstrates how to define a simple rectangular region of interest.
        /// </summary>
        public static void BasicRectangularROI()
        {
            // Load your image (using SkiaSharp as example)
            // var skBitmap = SKBitmap.Decode("input.png");
            
            // Create parameter list for encoding
            var parameters = new ParameterList();
            
            // Define a rectangular ROI at position (100, 50) with size 300x200
            // Format: R <left> <top> <width> <height>
            parameters["Rroi"] = "R 100 50 300 200";
            
            // Set compression rate (bits per pixel)
            parameters["rate"] = "1.0";
            
            // Enable file format wrapper
            parameters["file_format"] = "on";
            
            // Encode the image
            // byte[] encodedData = J2kImage.ToBytes(skBitmap, parameters);
            
            // Save to file
            // File.WriteAllBytes("output_with_roi.jp2", encodedData);
            
            Console.WriteLine("Encoded image with rectangular ROI:");
            Console.WriteLine("- Position: (100, 50)");
            Console.WriteLine("- Size: 300x200 pixels");
            Console.WriteLine("- ROI area will maintain higher quality");
        }

        /// <summary>
        /// Multiple ROIs example.
        /// Shows how to define multiple regions of interest in a single image.
        /// </summary>
        public static void MultipleROIs()
        {
            var parameters = new ParameterList();
            
            // Define three ROIs: two rectangles and one circle
            // Each R, C, or A starts a new ROI definition
            parameters["Rroi"] = "R 50 50 100 100 R 200 150 150 100 C 400 300 75";
            
            // Lower bitrate to see ROI effect more clearly
            parameters["rate"] = "0.5";
            
            // Enable file format
            parameters["file_format"] = "on";
            
            // Encode with multiple ROIs
            // byte[] encodedData = J2kImage.ToBytes(image, parameters);
            
            Console.WriteLine("Encoded image with multiple ROIs:");
            Console.WriteLine("- ROI 1: Rectangle at (50,50), size 100x100");
            Console.WriteLine("- ROI 2: Rectangle at (200,150), size 150x100");
            Console.WriteLine("- ROI 3: Circle at (400,300), radius 75");
        }

        /// <summary>
        /// Circular ROI example.
        /// Demonstrates circular regions of interest for radial importance.
        /// </summary>
        public static void CircularROI()
        {
            var parameters = new ParameterList();
            
            // Define a circular ROI
            // Format: C <center_x> <center_y> <radius>
            parameters["Rroi"] = "C 256 256 128";
            
            parameters["rate"] = "0.75";
            parameters["file_format"] = "on";
            
            // byte[] encodedData = J2kImage.ToBytes(image, parameters);
            
            Console.WriteLine("Encoded image with circular ROI:");
            Console.WriteLine("- Center: (256, 256)");
            Console.WriteLine("- Radius: 128 pixels");
        }

        /// <summary>
        /// Component-specific ROI example.
        /// Shows how to apply ROI to specific color components only.
        /// </summary>
        public static void ComponentSpecificROI()
        {
            var parameters = new ParameterList();
            
            // Apply ROI only to luminance component (component 0)
            // This preserves detail in brightness while allowing color to compress more
            parameters["Rroi"] = "0 R 100 100 300 200";
            
            // For RGB images converted with color transform:
            // Component 0 = Y (luminance)
            // Component 1 = Cb (blue chrominance)
            // Component 2 = Cr (red chrominance)
            
            parameters["rate"] = "1.0";
            parameters["file_format"] = "on";
            
            // byte[] encodedData = J2kImage.ToBytes(image, parameters);
            
            Console.WriteLine("Encoded image with component-specific ROI:");
            Console.WriteLine("- ROI applied only to component 0 (luminance)");
            Console.WriteLine("- Preserves brightness detail in ROI region");
        }

        /// <summary>
        /// Block-aligned ROI for performance.
        /// Demonstrates using block-aligned mode for better encoding efficiency.
        /// </summary>
        public static void BlockAlignedROI()
        {
            var parameters = new ParameterList();
            
            // Define ROI
            parameters["Rroi"] = "R 64 64 256 256";
            
            // Enable block-aligned mode
            // ROI boundaries will snap to code-block boundaries
            parameters["Ralign"] = "on";
            
            parameters["rate"] = "0.5";
            parameters["file_format"] = "on";
            
            // byte[] encodedData = J2kImage.ToBytes(image, parameters);
            
            Console.WriteLine("Encoded image with block-aligned ROI:");
            Console.WriteLine("- ROI boundaries aligned to code-blocks");
            Console.WriteLine("- Better encoding performance");
            Console.WriteLine("- Slightly less precise ROI boundaries");
        }

        /// <summary>
        /// ROI with resolution level inclusion.
        /// Ensures ROI is visible even at low resolutions.
        /// </summary>
        public static void ROIWithResolutionLevels()
        {
            var parameters = new ParameterList();
            
            parameters["Rroi"] = "R 100 100 400 300";
            
            // Include lowest 2 resolution levels in ROI
            // Ensures ROI is visible during progressive transmission
            parameters["Rstart_level"] = "1";
            
            parameters["rate"] = "0.75";
            parameters["file_format"] = "on";
            
            // byte[] encodedData = J2kImage.ToBytes(image, parameters);
            
            Console.WriteLine("Encoded image with resolution level ROI:");
            Console.WriteLine("- Lowest 2 resolution levels included in ROI");
            Console.WriteLine("- ROI visible even at reduced resolutions");
        }

        /// <summary>
        /// Arbitrary shape ROI using PGM mask.
        /// Shows how to use a grayscale mask file for complex ROI shapes.
        /// </summary>
        public static void ArbitraryShapeROI()
        {
            // First, create a PGM mask file
            // Non-zero pixels indicate ROI regions
            CreateSampleROIMask("roi_mask.pgm", 512, 512);
            
            var parameters = new ParameterList();
            
            // Use arbitrary ROI from PGM file
            // Format: A <filename>
            parameters["Rroi"] = "A roi_mask.pgm";
            
            parameters["rate"] = "1.0";
            parameters["file_format"] = "on";
            
            // byte[] encodedData = J2kImage.ToBytes(image, parameters);
            
            Console.WriteLine("Encoded image with arbitrary shape ROI:");
            Console.WriteLine("- ROI defined by PGM mask file");
            Console.WriteLine("- Non-zero pixels in mask indicate ROI");
            Console.WriteLine("- Allows complex, non-geometric shapes");
        }

        /// <summary>
        /// Medical imaging example with multiple ROIs.
        /// Demonstrates prioritizing diagnostic regions in medical images.
        /// </summary>
        public static void MedicalImagingROI()
        {
            var parameters = new ParameterList();
            
            // Define ROIs for different diagnostic regions
            // Primary lesion area - highest priority
            string roi1 = "R 200 150 100 100";
            
            // Secondary area of interest
            string roi2 = "R 350 200 80 80";
            
            // Anatomical landmark for reference
            string roi3 = "C 150 300 50";
            
            parameters["Rroi"] = $"{roi1} {roi2} {roi3}";
            
            // Use lower bitrate to simulate bandwidth constraints
            parameters["rate"] = "0.3";
            
            // Enable block alignment for performance
            parameters["Ralign"] = "on";
            
            parameters["file_format"] = "on";
            
            // byte[] encodedData = J2kImage.ToBytes(image, parameters);
            
            Console.WriteLine("Medical imaging ROI example:");
            Console.WriteLine("- Primary diagnostic region: 200,150 (100x100)");
            Console.WriteLine("- Secondary region: 350,200 (80x80)");
            Console.WriteLine("- Reference landmark: circle at 150,300 r=50");
            Console.WriteLine("- Low bitrate with ROI ensures diagnostic quality");
        }

        /// <summary>
        /// Document imaging example focusing on text regions.
        /// </summary>
        public static void DocumentImagingROI()
        {
            var parameters = new ParameterList();
            
            // Define ROIs for text regions in a document
            // Header/title area
            string headerROI = "R 50 20 700 100";
            
            // Main text body
            string bodyROI = "R 50 150 700 800";
            
            // Important footer/signature area
            string footerROI = "R 500 950 250 80";
            
            parameters["Rroi"] = $"{headerROI} {bodyROI} {footerROI}";
            
            // Use lossless compression for text (when rate = -1)
            // Or use high quality lossy
            parameters["rate"] = "2.0";
            
            parameters["file_format"] = "on";
            
            // byte[] encodedData = J2kImage.ToBytes(image, parameters);
            
            Console.WriteLine("Document imaging ROI example:");
            Console.WriteLine("- Header region protected");
            Console.WriteLine("- Main text body prioritized");
            Console.WriteLine("- Signature area preserved");
        }

        /// <summary>
        /// Helper method to create a sample PGM mask file for arbitrary ROI.
        /// </summary>
        private static void CreateSampleROIMask(string filename, int width, int height)
        {
            using (var writer = new StreamWriter(filename))
            {
                // PGM header
                writer.WriteLine("P2");  // ASCII grayscale
                writer.WriteLine($"{width} {height}");
                writer.WriteLine("255");  // Max value
                
                // Create a sample mask with a diagonal stripe as ROI
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Create diagonal stripe pattern
                        bool inROI = Math.Abs(x - y) < 50;
                        writer.Write(inROI ? "255 " : "0 ");
                    }
                    writer.WriteLine();
                }
            }
        }

        /// <summary>
        /// Complete example showing full encoding workflow with ROI.
        /// </summary>
        public static void CompleteROIWorkflow()
        {
            Console.WriteLine("Complete ROI Encoding Workflow:");
            Console.WriteLine("==============================");
            
            // Step 1: Load source image
            Console.WriteLine("1. Load source image...");
            // var sourceImage = SKBitmap.Decode("input.png");
            
            // Step 2: Set up encoding parameters
            Console.WriteLine("2. Configure encoding parameters...");
            var parameters = new ParameterList();
            
            // Basic encoding settings
            parameters["rate"] = "1.0";          // Target bitrate
            parameters["file_format"] = "on";     // Use JP2 format
            parameters["Qtype"] = "expounded";    // Quantization type
            parameters["Cblksiz"] = "64 64";      // Code-block size
            
            // ROI settings
            parameters["Rroi"] = "R 100 100 300 200 C 500 300 100";  // Multiple ROIs
            parameters["Ralign"] = "on";          // Block-aligned for performance
            parameters["Rstart_level"] = "0";     // Include lowest resolution
            
            // Step 3: Encode
            Console.WriteLine("3. Encoding with ROI...");
            // byte[] encodedData = J2kImage.ToBytes(sourceImage, parameters);
            
            // Step 4: Save result
            Console.WriteLine("4. Save encoded file...");
            // File.WriteAllBytes("output_roi.jp2", encodedData);
            
            Console.WriteLine("\nEncoding complete!");
            Console.WriteLine("ROI regions will maintain higher quality");
            Console.WriteLine("Background areas compressed more aggressively");
        }

        /// <summary>
        /// Main method to run all examples.
        /// </summary>
        public static void Main(string[] args)
        {
            Console.WriteLine("CoreJ2K ROI Encoding Examples");
            Console.WriteLine("=============================\n");
            
            // Run examples (uncomment to execute with actual images)
            // BasicRectangularROI();
            // MultipleROIs();
            // CircularROI();
            // ComponentSpecificROI();
            // BlockAlignedROI();
            // ROIWithResolutionLevels();
            // ArbitraryShapeROI();
            // MedicalImagingROI();
            // DocumentImagingROI();
            // CompleteROIWorkflow();
            
            Console.WriteLine("\nNote: Examples show parameter usage.");
            Console.WriteLine("Uncomment code and provide actual images to run.");
        }
    }
}