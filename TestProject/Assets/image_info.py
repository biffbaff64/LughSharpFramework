from PIL import Image

try:
    image_path = "title_background.png"  # Replace with your image path
    
    with Image.open(image_path) as img:
        mode = img.mode
        
        # Check for bit depth info in the image.info dictionary
        bit_depth_info = img.info.get("bits")

        print(f"File: {image_path}")
        print(f"Pillow Mode: {mode}")

        # Check if bits information is available
        if bit_depth_info:
            # The 'bits' info is often a tuple like (8, 8, 8, 8)
            if isinstance(bit_depth_info, tuple) and len(bit_depth_info) == len(mode):
                formatted_depth = ''.join(map(str, bit_depth_info))
                print(f"Image Bit Depth: RGBA{formatted_depth}")
            else:
                print(f"Image Bit Depth: {bit_depth_info} bits per channel (or an unknown format)")
        else:
            # If no specific 'bits' info, assume standard 8-bit for common modes
            if mode == 'RGBA':
                print("Image Bit Depth: RGBA8888 (Standard)")
            elif mode == 'RGB':
                print("Image Bit Depth: RGB888 (Standard)")
            elif mode == 'L':
                print("Image Bit Depth: 8-bit Grayscale (L8)")
            else:
                print("Image Bit Depth: Format details are not explicitly available.")

except FileNotFoundError:
    print(f"Error: The file '{image_path}' was not found.")
except Exception as e:
    print(f"An error occurred: {e}")

