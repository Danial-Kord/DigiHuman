using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FreeDraw
{
    // [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]  // REQUIRES A COLLIDER2D to function
    // 1. Attach this to a read/write enabled sprite image
    // 2. Set the drawing_layers  to use in the raycast
    // 3. Attach a 2D collider (like a Box Collider 2D) to this sprite
    // 4. Hold down left mouse to draw on this texture!
    public class Drawable : MonoBehaviour
    {
        // PEN COLOUR
        public static Color Pen_Colour = Color.red;     // Change these to change the default drawing settings
        // PEN WIDTH (actually, it's a radius, in pixels)
        public static int Pen_Width = 3;


        [Tooltip("1 --> image canvas as paint plane,\n2--> sprite renderer as paint plane")]
        public int paintMethod = 1;
        public delegate void Brush_Function(Vector2 world_position);
        // This is the function called when a left click happens
        // Pass in your own custom one to change the brush type
        // Set the default function in the Awake method
        public Brush_Function current_brush;

        public LayerMask Drawing_Layers;

        public bool Reset_Canvas_On_Play = true;
        // The colour the canvas is reset to each time
        public Color Reset_Colour = new Color(0, 0, 0, 0);  // By default, reset the canvas to be transparent

        // Used to reference THIS specific file without making all methods static
        public static Drawable drawable;
        
        // MUST HAVE READ/WRITE enabled set in the file editor of Unity
        public Sprite drawable_sprite;
        
        
        [Header("For method 1 only")]
        public Transform imagePivotPos;

        private float imageWidth;
        private float imageHeight;
        
        Texture2D drawable_texture;

        Vector2 previous_drag_position;
        Color[] clean_colours_array;
        Color transparent;
        Color32[] cur_colors;
        bool mouse_was_previously_held_down = false;
        bool no_drawing_on_current_drag = false;



//////////////////////////////////////////////////////////////////////////////
// BRUSH TYPES. Implement your own here


        public void BrushClosedArea(Vector2 world_point)
        {
            Vector2 pixel_pos = WorldToPixelCoordinates(world_point);
            Color32 targetColor = Pen_Colour;
            cur_colors = drawable_texture.GetPixels32();
            
            Queue<Vector2> pixelsToBeColored = new Queue<Vector2>(); //our queue for coloring un colored pixels
            HashSet<Vector2> coloredPixels = new HashSet<Vector2>(); //our colored hashset of pixels
            pixelsToBeColored.Enqueue(pixel_pos);
            coloredPixels.Add(pixel_pos);

            
            Color32 baseColor = cur_colors[(int)pixel_pos.y * (int)drawable_sprite.rect.width + (int)pixel_pos.x];
            
            if(baseColor.Equals(targetColor))
                return;
            while (pixelsToBeColored.Count != 0)
            {
                Vector2 currentPixel = pixelsToBeColored.Dequeue();
                int x = (int) currentPixel.x;
                int y = (int) currentPixel.y;

                if(!cur_colors[y * (int)drawable_sprite.rect.width + x].Equals(baseColor))//color is not the same so skip
                    continue;
                MarkPixelToChange(x, y, targetColor);
                
                Vector2 up = new Vector2(x,y-1);
                if (y - 1 >= 0)
                {
                    if(coloredPixels.Add(up))
                        pixelsToBeColored.Enqueue(up);
                }

                Vector2 down = new Vector2(x,y+1);
                if (y+1 < (int)drawable_sprite.rect.height)
                {
                    if(coloredPixels.Add(down))
                        pixelsToBeColored.Enqueue(down);
                }
                
                Vector2 right = new Vector2(x+1,y);
                if (x+1 < (int)drawable_sprite.rect.width)
                {
                    if(coloredPixels.Add(right))
                        pixelsToBeColored.Enqueue(right);
                }
                Vector2 left = new Vector2(x-1,y);
                if (x-1 >= 0)
                {
                    if(coloredPixels.Add(left))
                        pixelsToBeColored.Enqueue(left);
                }

            }
            ApplyMarkedPixelChanges();
        }


        // When you want to make your own type of brush effects,
        // Copy, paste and rename this function.
        // Go through each step
        public void BrushTemplate(Vector2 world_position)
        {
            // 1. Change world position to pixel coordinates
            Vector2 pixel_pos = WorldToPixelCoordinates(world_position);

            // 2. Make sure our variable for pixel array is updated in this frame
            cur_colors = drawable_texture.GetPixels32();

            ////////////////////////////////////////////////////////////////
            // FILL IN CODE BELOW HERE

            // Do we care about the user left clicking and dragging?
            // If you don't, simply set the below if statement to be:
            //if (true)

            // If you do care about dragging, use the below if/else structure
            if (previous_drag_position == Vector2.zero)
            {
                // THIS IS THE FIRST CLICK
                // FILL IN WHATEVER YOU WANT TO DO HERE
                // Maybe mark multiple pixels to colour?
                MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
            }
            else
            {
                // THE USER IS DRAGGING
                // Should we do stuff between the previous mouse position and the current one?
                ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
            }
            ////////////////////////////////////////////////////////////////

            // 3. Actually apply the changes we marked earlier
            // Done here to be more efficient
            ApplyMarkedPixelChanges();
            
            // 4. If dragging, update where we were previously
            previous_drag_position = pixel_pos;
        }



        
        // Default brush type. Has width and colour.
        // Pass in a point in WORLD coordinates
        // Changes the surrounding pixels of the world_point to the static pen_colour
        public void PenBrush(Vector2 world_point)
        {
            Vector2 pixel_pos = WorldToPixelCoordinates(world_point);

            cur_colors = drawable_texture.GetPixels32();

            if (previous_drag_position == Vector2.zero)
            {
                // If this is the first time we've ever dragged on this image, simply colour the pixels at our mouse position
                MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
            }
            else
            {
                // Colour in a line from where we were on the last update call
                ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
            }
            ApplyMarkedPixelChanges();

            //Debug.Log("Dimensions: " + pixelWidth + "," + pixelHeight + ". Units to pixels: " + unitsToPixels + ". Pixel pos: " + pixel_pos);
            previous_drag_position = pixel_pos;
        }


        // Helper method used by UI to set what brush the user wants
        // Create a new one for any new brushes you implement
        public void SetPenBrush()
        {
            // PenBrush is the NAME of the method we want to set as our current brush
            current_brush = PenBrush;
        }
//////////////////////////////////////////////////////////////////////////////





        
        // This is where the magic happens.
        // Detects when user is left clicking, which then call the appropriate function
        void Update()
        {

            
            bool mouse_held_down = Input.GetMouseButton(0);
            if (mouse_held_down && !no_drawing_on_current_drag)
            {
                print("fuk");
            
            
                // Convert mouse coordinates to world coordinates
                Vector2 mouse_world_position = Input.mousePosition;

                // testImage.transform.position = mouse_world_position;
                var posInImage = (Input.mousePosition - imagePivotPos.position);
            
                if (posInImage.x >= 0 && posInImage.x < imageWidth &&
                    posInImage.y <=0  && -posInImage.y < imageHeight)
                {
                    print("fuk2");
            
                    current_brush(mouse_world_position);
            
                }
                else
                {
                    print("fuk3");
            
                    // We're not over our destination texture
                    previous_drag_position = Vector2.zero;
                    if (!mouse_was_previously_held_down)
                    {
                        // This is a new drag where the user is left clicking off the canvas
                        // Ensure no drawing happens until a new drag is started
                        no_drawing_on_current_drag = true;
                    }
                }
                
                // Check if the current mouse position overlaps our image
                Collider2D hit = Physics2D.OverlapPoint(mouse_world_position, Drawing_Layers.value);
                if (hit != null && hit.transform != null)
                {
                    // We're over the texture we're drawing on!
                    // Use whatever function the current brush is
                    //current_brush(mouse_world_position);
                }
            
                else
                {
                    // We're not over our destination texture
                    // previous_drag_position = Vector2.zero;
                    // if (!mouse_was_previously_held_down)
                    // {
                    //     // This is a new drag where the user is left clicking off the canvas
                    //     // Ensure no drawing happens until a new drag is started
                    //     no_drawing_on_current_drag = true;
                    // }
                }
            }
            // Mouse is released
            else if (!mouse_held_down)
            {
                previous_drag_position = Vector2.zero;
                no_drawing_on_current_drag = false;
            }
            mouse_was_previously_held_down = mouse_held_down;
            
            

            // Is the user holding down the left mouse button?
            // bool mouse_held_down = Input.GetMouseButton(0);
            // if (mouse_held_down && !no_drawing_on_current_drag)
            // {
            //     // Convert mouse coordinates to world coordinates
            //     Vector2 mouse_world_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //
            //     // Check if the current mouse position overlaps our image
            //     Collider2D hit = Physics2D.OverlapPoint(mouse_world_position, Drawing_Layers.value);
            //     if (hit != null && hit.transform != null)
            //     {
            //         // We're over the texture we're drawing on!
            //         // Use whatever function the current brush is
            //         current_brush(mouse_world_position);
            //     }
            //
            //     else
            //     {
            //         // We're not over our destination texture
            //         previous_drag_position = Vector2.zero;
            //         if (!mouse_was_previously_held_down)
            //         {
            //             // This is a new drag where the user is left clicking off the canvas
            //             // Ensure no drawing happens until a new drag is started
            //             no_drawing_on_current_drag = true;
            //         }
            //     }
            // }
            // // Mouse is released
            // else if (!mouse_held_down)
            // {
            //     previous_drag_position = Vector2.zero;
            //     no_drawing_on_current_drag = false;
            // }
            // mouse_was_previously_held_down = mouse_held_down;
            //
        }



        // Set the colour of pixels in a straight line from start_point all the way to end_point, to ensure everything inbetween is coloured
        public void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
        {
            // Get the distance from start to finish
            float distance = Vector2.Distance(start_point, end_point);
            Vector2 direction = (start_point - end_point).normalized;

            Vector2 cur_position = start_point;

            // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
            float lerp_steps = 1 / distance;

            for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
            {
                cur_position = Vector2.Lerp(start_point, end_point, lerp);
                MarkPixelsToColour(cur_position, width, color);
            }
        }





        public void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
        {
            // Figure out how many pixels we need to colour in each direction (x and y)
            int center_x = (int)center_pixel.x;
            int center_y = (int)center_pixel.y;
            //int extra_radius = Mathf.Min(0, pen_thickness - 2);

            for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
            {
                // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
                if (x >= (int)drawable_sprite.rect.width || x < 0)
                    continue;

                for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
                {
                    MarkPixelToChange(x, y, color_of_pen);
                }
            }
        }
        public void MarkPixelToChange(int x, int y, Color color)
        {
            // Need to transform x and y coordinates to flat coordinates of array
            int array_pos = y * (int)drawable_sprite.rect.width + x;

            // Check if this is a valid position
            if (array_pos > cur_colors.Length || array_pos < 0)
                return;

            cur_colors[array_pos] = color;
        }
        public void ApplyMarkedPixelChanges()
        {
            drawable_texture.SetPixels32(cur_colors);
            drawable_texture.Apply();
        }


        // Directly colours pixels. This method is slower than using MarkPixelsToColour then using ApplyMarkedPixelChanges
        // SetPixels32 is far faster than SetPixel
        // Colours both the center pixel, and a number of pixels around the center pixel based on pen_thickness (pen radius)
        public void ColourPixels(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
        {
            // Figure out how many pixels we need to colour in each direction (x and y)
            int center_x = (int)center_pixel.x;
            int center_y = (int)center_pixel.y;
            //int extra_radius = Mathf.Min(0, pen_thickness - 2);

            for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
            {
                for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
                {
                    drawable_texture.SetPixel(x, y, color_of_pen);
                }
            }

            drawable_texture.Apply();
        }

        
        public Vector2 WorldToPixelCoordinates(Vector2 world_position)
        {
            //method 1 default --> use canvas image sprite
            if (paintMethod == 1)
            {
                return WorldToPixelCoordinatesCanvas(world_position);
            }

            if (paintMethod == 2)
            {
                return WorldToPixelCoordinatesSprite(world_position);
            }

            return Vector2.zero;
        }
        

        //for sprite renderer setup
        public Vector2 WorldToPixelCoordinatesSprite(Vector2 world_position)
        {
            // Change coordinates to local coordinates of this image
            Vector3 local_pos = transform.InverseTransformPoint(world_position);
            // Change these to coordinates of pixels
            float pixelWidth = drawable_sprite.rect.width;
            float pixelHeight = drawable_sprite.rect.height;

            float unitsToPixelsX = pixelWidth / drawable_sprite.bounds.size.x;
            float unitsToPixelsH = pixelHeight / drawable_sprite.bounds.size.y;

            // Need to center our coordinates
            float centered_x = local_pos.x * unitsToPixelsX + pixelWidth / 2;
            float centered_y = local_pos.y * unitsToPixelsH + pixelHeight / 2;

            // Round current mouse position to nearest pixel
            Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

            print(world_position +" " + local_pos + " " + pixel_pos);

            return pixel_pos;
        }

        //for canvas image setup
        public Vector2 WorldToPixelCoordinatesCanvas(Vector2 world_position)
        {
            // Change coordinates to local coordinates of this image
            Vector3 local_pos = transform.InverseTransformPoint(world_position);
            local_pos.y += imageHeight;

            float y_normalize = local_pos.y / imageHeight;
            float x_normalize = local_pos.x / imageWidth;
            
            // Change these to coordinates of pixels
            float pixelWidth = drawable_sprite.rect.width;
            float pixelHeight = drawable_sprite.rect.height;
            
            return new Vector2(x_normalize *pixelWidth , y_normalize*pixelHeight);
        }

        
        
        // Changes every pixel to be the reset colour
        public void ResetCanvas()
        {
            drawable_texture.SetPixels(clean_colours_array);
            drawable_texture.Apply();
        }

        public string SaveTexturePng()
        {
            
            byte[] bytes = drawable_texture.EncodeToPNG();
            var dirPath = FileManager.SketchDir;
            dirPath = dirPath + "/R_" + Random.Range(0, 100000) + ".png";
            FileManager.SaveBinary(dirPath,bytes);
            return dirPath;
        }
        

        public void SetBrushModeArea()
        {
            current_brush = BrushClosedArea;
        }
        
        void Awake()
        {
            drawable = this;
            // DEFAULT BRUSH SET HERE
            current_brush = PenBrush;

            if (paintMethod == 1)
            {
                drawable_sprite = GetComponent<Image>().sprite;
                imageHeight = GetComponent<RectTransform>().rect.height;
                imageWidth = GetComponent<RectTransform>().rect.width;
            }
            else
            {
                drawable_sprite = GetComponent<SpriteRenderer>().sprite;

            }
            drawable_texture = drawable_sprite.texture;

            // Initialize clean pixels to use
            clean_colours_array = new Color[(int)drawable_sprite.rect.width * (int)drawable_sprite.rect.height];
            for (int x = 0; x < clean_colours_array.Length; x++)
                clean_colours_array[x] = Reset_Colour;

            // Should we reset our canvas image when we hit play in the editor?
            if (Reset_Canvas_On_Play)
                ResetCanvas();
        }
    }
}