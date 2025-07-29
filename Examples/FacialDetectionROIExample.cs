using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CoreJ2K;
using CoreJ2K.j2k.util;
using CoreJ2K.Util;

namespace CoreJ2K.Examples
{
    /// <summary>
    /// Example demonstrating ROI encoding for facial recognition applications
    /// using the 68-point facial landmark model.
    /// </summary>
    public class FacialDetectionROIExample
    {
        /// <summary>
        /// Represents a 2D point for facial landmarks.
        /// </summary>
        public struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }
            
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        
        /// <summary>
        /// Example of encoding an image with facial ROI based on 68-point landmarks.
        /// </summary>
        public static void EncodeFacialImage()
        {
            // In a real application, these landmarks would come from a facial detection library
            // like dlib, MediaPipe, or OpenCV
            Point[] landmarks = GenerateExampleLandmarks();
            
            // Generate ROI specification from landmarks
            string roiSpec = GenerateFacialROI(landmarks);
            
            // Set up encoding parameters
            var parameters = new ParameterList();
            parameters["Rroi"] = roiSpec;
            parameters["rate"] = "1.0";  // 1 bit per pixel
            parameters["Ralign"] = "on"; // Block-aligned for better performance
            parameters["file_format"] = "on";
            
            Console.WriteLine("Facial ROI Encoding Example");
            Console.WriteLine("==========================");
            Console.WriteLine($"Generated ROI specification: {roiSpec}");
            Console.WriteLine("\nROI Regions:");
            Console.WriteLine("- Region 1 (Eyes & Eyebrows): Highest priority");
            Console.WriteLine("- Region 2 (Nose, Mouth & Jaw): Medium priority");
            Console.WriteLine("- Region 3 (Full Face + Context): Standard compression");
            
            // In a real application:
            // byte[] encoded = J2kImage.ToBytes(faceImage, parameters);
            // File.WriteAllBytes("face_with_roi.jp2", encoded);
        }
        
        /// <summary>
        /// Convert 68 facial landmark points to ROI regions for JPEG2000 encoding.
        /// </summary>
        /// <param name="landmarks">Array of 68 facial landmark points</param>
        /// <returns>ROI specification string for CoreJ2K</returns>
        public static string GenerateFacialROI(Point[] landmarks)
        {
            if (landmarks.Length != 68)
            {
                throw new ArgumentException("Expected 68 landmark points");
            }
            
            // Region 1: Eyes and Eyebrows (points 17-26, 36-47)
            var eyesAndBrows = new List<int>();
            for (int i = 17; i <= 26; i++) eyesAndBrows.Add(i); // Eyebrows
            for (int i = 36; i <= 47; i++) eyesAndBrows.Add(i); // Eyes
            Rectangle eyesBox = CalculateBoundingBox(landmarks, eyesAndBrows.ToArray(), 0.1);
            
            // Region 2: Nose, Mouth, and Jaw (points 0-16, 27-35, 48-67)
            var lowerFace = new List<int>();
            for (int i = 0; i <= 16; i++) lowerFace.Add(i);   // Jaw
            for (int i = 27; i <= 35; i++) lowerFace.Add(i);  // Nose
            for (int i = 48; i <= 67; i++) lowerFace.Add(i);  // Mouth
            Rectangle lowerFaceBox = CalculateBoundingBox(landmarks, lowerFace.ToArray(), 0.1);
            
            // Region 3: Full face with context (all points with 20% padding)
            var allPoints = Enumerable.Range(0, 68).ToArray();
            Rectangle fullFaceBox = CalculateBoundingBox(landmarks, allPoints, 0.2);
            
            // Format as ROI parameters
            // Order matters: define from highest to lowest priority
            return string.Format("R {0} {1} {2} {3} R {4} {5} {6} {7} R {8} {9} {10} {11}",
                eyesBox.X, eyesBox.Y, eyesBox.Width, eyesBox.Height,
                lowerFaceBox.X, lowerFaceBox.Y, lowerFaceBox.Width, lowerFaceBox.Height,
                fullFaceBox.X, fullFaceBox.Y, fullFaceBox.Width, fullFaceBox.Height);
        }
        
        /// <summary>
        /// Calculate bounding box from specified landmark points with padding.
        /// </summary>
        private static Rectangle CalculateBoundingBox(Point[] landmarks, int[] indices, double paddingRatio)
        {
            var points = indices.Select(i => landmarks[i]).ToList();
            
            if (!points.Any())
            {
                throw new ArgumentException("No points specified for bounding box");
            }
            
            int minX = points.Min(p => p.X);
            int minY = points.Min(p => p.Y);
            int maxX = points.Max(p => p.X);
            int maxY = points.Max(p => p.Y);
            
            int width = maxX - minX;
            int height = maxY - minY;
            
            // Add padding
            int padX = (int)(width * paddingRatio);
            int padY = (int)(height * paddingRatio);
            
            return new Rectangle(
                minX - padX, 
                minY - padY,
                width + 2 * padX, 
                height + 2 * padY);
        }
        
        /// <summary>
        /// Generate example 68 landmark points for a typical face.
        /// In practice, these would come from a facial detection library.
        /// </summary>
        private static Point[] GenerateExampleLandmarks()
        {
            // This generates approximate landmark positions for a face
            // centered at (256, 256) with reasonable proportions
            Point[] landmarks = new Point[68];
            
            // Jawline (0-16)
            int jawY = 380;
            int jawTopY = 256;
            for (int i = 0; i <= 16; i++)
            {
                double angle = Math.PI * (1.0 - (double)i / 16.0);
                int x = 256 + (int)(120 * Math.Cos(angle));
                int y = i == 8 ? jawY : jawTopY + (int)((jawY - jawTopY) * Math.Sin(angle));
                landmarks[i] = new Point(x, y);
            }
            
            // Right eyebrow (17-21)
            for (int i = 17; i <= 21; i++)
            {
                int x = 200 + (i - 17) * 15;
                int y = 200 - (int)(10 * Math.Sin((i - 17) * Math.PI / 4));
                landmarks[i] = new Point(x, y);
            }
            
            // Left eyebrow (22-26)
            for (int i = 22; i <= 26; i++)
            {
                int x = 280 + (i - 22) * 15;
                int y = 200 - (int)(10 * Math.Sin((i - 22) * Math.PI / 4));
                landmarks[i] = new Point(x, y);
            }
            
            // Nose (27-35)
            // Nose bridge (27-30)
            for (int i = 27; i <= 30; i++)
            {
                landmarks[i] = new Point(256, 220 + (i - 27) * 20);
            }
            // Nose bottom (31-35)
            landmarks[31] = new Point(236, 280);
            landmarks[32] = new Point(246, 285);
            landmarks[33] = new Point(256, 287);
            landmarks[34] = new Point(266, 285);
            landmarks[35] = new Point(276, 280);
            
            // Right eye (36-41)
            double[] rightEyeAngles = { 0, 30, 60, 120, 150, 180 };
            for (int i = 0; i < 6; i++)
            {
                double angle = rightEyeAngles[i] * Math.PI / 180;
                landmarks[36 + i] = new Point(
                    210 + (int)(25 * Math.Cos(angle)),
                    220 + (int)(10 * Math.Sin(angle)));
            }
            
            // Left eye (42-47)
            double[] leftEyeAngles = { 0, 30, 60, 120, 150, 180 };
            for (int i = 0; i < 6; i++)
            {
                double angle = leftEyeAngles[i] * Math.PI / 180;
                landmarks[42 + i] = new Point(
                    302 + (int)(25 * Math.Cos(angle)),
                    220 + (int)(10 * Math.Sin(angle)));
            }
            
            // Outer mouth (48-59)
            for (int i = 0; i < 12; i++)
            {
                double angle = i * Math.PI / 6;
                landmarks[48 + i] = new Point(
                    256 + (int)(40 * Math.Cos(angle)),
                    320 + (int)(20 * Math.Sin(angle)));
            }
            
            // Inner mouth (60-67)
            for (int i = 0; i < 8; i++)
            {
                double angle = i * Math.PI / 4;
                landmarks[60 + i] = new Point(
                    256 + (int)(25 * Math.Cos(angle)),
                    320 + (int)(12 * Math.Sin(angle)));
            }
            
            return landmarks;
        }
        
        /// <summary>
        /// Example showing different compression ratios for facial regions.
        /// </summary>
        public static void DemonstrateDifferentialCompression()
        {
            Point[] landmarks = GenerateExampleLandmarks();
            
            // Example 1: High quality for biometric applications
            var biometricParams = new ParameterList();
            biometricParams["Rroi"] = GenerateFacialROI(landmarks);
            biometricParams["rate"] = "2.0"; // Higher bitrate for quality
            biometricParams["Ralign"] = "off"; // Precise boundaries
            biometricParams["file_format"] = "on";
            
            Console.WriteLine("\nBiometric Quality Settings:");
            Console.WriteLine("- Rate: 2.0 bpp (high quality)");
            Console.WriteLine("- Block alignment: OFF (precise ROI boundaries)");
            Console.WriteLine("- Use case: Identity verification, security");
            
            // Example 2: Balanced quality for general use
            var balancedParams = new ParameterList();
            balancedParams["Rroi"] = GenerateFacialROI(landmarks);
            balancedParams["rate"] = "1.0"; // Moderate bitrate
            balancedParams["Ralign"] = "on"; // Better performance
            balancedParams["file_format"] = "on";
            
            Console.WriteLine("\nBalanced Quality Settings:");
            Console.WriteLine("- Rate: 1.0 bpp (balanced quality/size)");
            Console.WriteLine("- Block alignment: ON (better performance)");
            Console.WriteLine("- Use case: Social media, general photography");
            
            // Example 3: Low bandwidth optimized
            var lowBandwidthParams = new ParameterList();
            lowBandwidthParams["Rroi"] = GenerateFacialROI(landmarks);
            lowBandwidthParams["rate"] = "0.5"; // Low bitrate
            lowBandwidthParams["Ralign"] = "on";
            lowBandwidthParams["Rstart_level"] = "1"; // Include low res in ROI
            lowBandwidthParams["file_format"] = "on";
            
            Console.WriteLine("\nLow Bandwidth Settings:");
            Console.WriteLine("- Rate: 0.5 bpp (aggressive compression)");
            Console.WriteLine("- Block alignment: ON");
            Console.WriteLine("- Resolution levels: Include low resolution in ROI");
            Console.WriteLine("- Use case: Video conferencing, real-time transmission");
        }
        
        /// <summary>
        /// Example of handling multiple faces in an image.
        /// </summary>
        public static void HandleMultipleFaces()
        {
            // Simulate detection of 3 faces
            Point[][] allFaces = new Point[3][];
            allFaces[0] = GenerateExampleLandmarks(); // Primary face
            allFaces[1] = GenerateExampleLandmarks(); // Secondary face (offset)
            allFaces[2] = GenerateExampleLandmarks(); // Tertiary face
            
            // Offset the secondary and tertiary faces
            for (int i = 0; i < 68; i++)
            {
                allFaces[1][i].X += 300;
                allFaces[2][i].X -= 200;
                allFaces[2][i].Y += 100;
            }
            
            // Generate ROI for all faces (limit to prevent performance issues)
            var roiParts = new List<string>();
            int maxFaces = Math.Min(allFaces.Length, 3); // Limit to 3 faces
            
            for (int i = 0; i < maxFaces; i++)
            {
                // For secondary faces, we might only encode the eye region
                if (i == 0)
                {
                    // Primary face gets full 3-region treatment
                    roiParts.Add(GenerateFacialROI(allFaces[i]));
                }
                else
                {
                    // Secondary faces get only eye region
                    var eyeIndices = Enumerable.Range(17, 10)
                                              .Concat(Enumerable.Range(36, 12))
                                              .ToArray();
                    Rectangle eyeBox = CalculateBoundingBox(allFaces[i], eyeIndices, 0.15);
                    roiParts.Add($"R {eyeBox.X} {eyeBox.Y} {eyeBox.Width} {eyeBox.Height}");
                }
            }
            
            var parameters = new ParameterList();
            parameters["Rroi"] = string.Join(" ", roiParts);
            parameters["rate"] = "1.0";
            parameters["Ralign"] = "on";
            parameters["file_format"] = "on";
            
            Console.WriteLine("\nMultiple Face Encoding:");
            Console.WriteLine($"- Detected {allFaces.Length} faces");
            Console.WriteLine($"- Encoding ROI for {maxFaces} faces");
            Console.WriteLine("- Primary face: Full 3-region ROI");
            Console.WriteLine("- Secondary faces: Eye region only");
        }
        
        /// <summary>
        /// Main method demonstrating various facial ROI encoding scenarios.
        /// </summary>
        public static void Main(string[] args)
        {
            Console.WriteLine("CoreJ2K Facial Detection ROI Examples");
            Console.WriteLine("=====================================\n");
            
            // Basic facial ROI encoding
            EncodeFacialImage();
            
            Console.WriteLine("\n" + new string('-', 50) + "\n");
            
            // Different quality settings
            DemonstrateDifferentialCompression();
            
            Console.WriteLine("\n" + new string('-', 50) + "\n");
            
            // Multiple faces
            HandleMultipleFaces();
            
            Console.WriteLine("\n\nNote: These examples demonstrate parameter usage.");
            Console.WriteLine("In production, integrate with facial detection libraries like:");
            Console.WriteLine("- dlib (68-point model)");
            Console.WriteLine("- MediaPipe");
            Console.WriteLine("- OpenCV");
            Console.WriteLine("- ONNX Runtime with appropriate models");
        }
    }
}