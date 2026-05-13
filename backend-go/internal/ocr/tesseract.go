// Package tesseract: OCR definition
package ocr

import (
	"fmt"

	"github.com/otiai10/gosseract/v2"
)

func ExtractText(imagemBytes []byte) (string, error) {
	processed, err := PreprocessImage(imagemBytes)
	if err != nil {
		return "", fmt.Errorf("preprocessing image: %w", err)
	}

	client := gosseract.NewClient()

	defer client.Close()

	if err := client.SetLanguage("por"); err != nil {
		return "", fmt.Errorf("language 'por' not found: %w", err)
	}

	// Tell Tesseract the expected DPI — it defaults to 70 if unknown,
	// which makes it misread character shapes on modern phone photos
	if err := client.SetVariable("user_defined_dpi", "300"); err != nil {
		return "", fmt.Errorf("setting dpi: %w", err)
	}

	err = client.SetImageFromBytes(processed)
	if err != nil {
		return "", fmt.Errorf("error loading image to tesseract: %w", err)
	}

	texto, err := client.Text()
	if err != nil {
		return "", fmt.Errorf("erro reading the text: %w", err)
	}

	return texto, nil
}
