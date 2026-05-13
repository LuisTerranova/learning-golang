package imaging

import (
	"bytes"
	"fmt"
	"image"
	"image/color"
	"image/png"

	"github.com/gen2brain/go-fitz"
)

func PrepareForOCR(data []byte) ([]byte, error) {
	doc, err := fitz.NewFromMemory(data)
	if err != nil {
		return data, nil
	}
	defer doc.Close()

	if doc.NumPage() == 0 {
		return nil, fmt.Errorf("PDF has no pages")
	}

	img, err := doc.ImageDPI(0, 300)
	if err != nil {
		return nil, fmt.Errorf("failed to render PDF page: %w", err)
	}

	gray := toGrayscale(img)

	var buf bytes.Buffer
	if err := png.Encode(&buf, gray); err != nil {
		return nil, fmt.Errorf("failed to encode image: %w", err)
	}
	return buf.Bytes(), nil
}

func toGrayscale(src image.Image) *image.Gray {
	bounds := src.Bounds()
	gray := image.NewGray(bounds)
	for y := bounds.Min.Y; y < bounds.Max.Y; y++ {
		for x := bounds.Min.X; x < bounds.Max.X; x++ {
			r, g, b, _ := src.At(x, y).RGBA()
			lum := uint8((19595*r + 38470*g + 7471*b + 1<<15) >> 24)
			gray.SetGray(x, y, color.Gray{Y: lum})
		}
	}
	return gray
}
