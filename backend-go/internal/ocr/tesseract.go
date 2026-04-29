// Package tesseract: OCR definition
package ocr

import (
	"fmt"

	"github.com/otiai10/gosseract/v2"
)

func ExtractText(imagemBytes []byte) (string, error) {
	client := gosseract.NewClient()

	// 2. Ensures the client will be closed at the end to prevent unwanted memory usage
	defer client.Close()

	// 3. Define language
	if err := client.SetLanguage("por"); err != nil {
		return "", fmt.Errorf("language 'por' not found: %w", err)
	}

	// 4. Pass the image
	err := client.SetImageFromBytes(imagemBytes)
	if err != nil {
		return "", fmt.Errorf("error loading image to tesseract: %w", err)
	}

	// 5. Read
	texto, err := client.Text()
	if err != nil {
		return "", fmt.Errorf("erro reading the text: %w", err)
	}

	return texto, nil
}
