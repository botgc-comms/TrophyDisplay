using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrophiesDisplay.Controllers
{
    public class FaceProcessor
    {
        public IList<DetectedFace> GetPrimaryFaces(
    IList<DetectedFace> detectedFaces,
    int imageWidth,
    int imageHeight)
        {
            if (detectedFaces == null || detectedFaces.Count < 2)
                throw new ArgumentException("At least 2 faces must be detected.");

            // Step 1: Calculate the area of each face
            var faceAreas = detectedFaces.Select(face =>
                face.FaceRectangle.Width * face.FaceRectangle.Height).ToList();

            // Step 2: Sort face areas (and their corresponding faces) in descending order
            var sortedFaces = detectedFaces
                .Zip(faceAreas, (face, area) => new { Face = face, Area = area })
                .OrderByDescending(item => item.Area)
                .ToList();

            // Step 3: Calculate differences between consecutive face sizes
            var sizeDifferences = sortedFaces
                .Select((item, index) => index < sortedFaces.Count - 1
                    ? sortedFaces[index].Area - sortedFaces[index + 1].Area
                    : 0) // No difference for the last face
                .ToList();

            // Step 4: Identify the largest jump
            int cutoffIndex = sizeDifferences.IndexOf(sizeDifferences.Max());

            // Step 5: Select primary faces (up to the cutoff index)
            var primaryFaces = sortedFaces
                .Take(cutoffIndex + 1)
                .Select(item => item.Face)
                .ToList();

            // Step 6: Ensure at least 2–3 primary faces
            if (primaryFaces.Count < 2)
            {
                primaryFaces = sortedFaces
                    .Take(3)
                    .Select(item => item.Face)
                    .ToList();
            }

            return primaryFaces;
        }

    }
}