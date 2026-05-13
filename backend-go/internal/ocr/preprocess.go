// Package preprocess: image preprocessing to improve OCR quality
package ocr

import (
	"bytes"
	"fmt"
	"image"
	"image/jpeg"
	"image/png"

	"github.com/disintegration/imaging"
)

func PreprocessImage(data []byte) ([]byte, error) {
	img, format, err := image.Decode(bytes.NewReader(data))
	if err != nil {
		return nil, fmt.Errorf("decoding image: %w", err)
	}

	// Grayscale — Tesseract works on luminance
	img = imaging.Grayscale(img)

	// Resize to at least 2000px wide if smaller — aims for ~300 DPI sweetspot
	if img.Bounds().Dx() < 2000 {
		img = imaging.Resize(img, 2000, 0, imaging.Lanczos)
	}

	// Re-encode preserving original format
	var buf bytes.Buffer
	switch format {
	case "png":
		err = png.Encode(&buf, img)
	default:
		err = jpeg.Encode(&buf, img, &jpeg.Options{Quality: 90})
	}
	if err != nil {
		return nil, fmt.Errorf("re-encoding image: %w", err)
	}

	return buf.Bytes(), nil
}
