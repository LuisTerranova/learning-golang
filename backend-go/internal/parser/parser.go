// Package parser: parser definition
package parser

import (
	"strconv"
	"strings"
	"time"

	"github.com/google/uuid"

	"github.com/LuisTerranova/invoices-app/backend-go/internal/models"
)

func Parse(rawText string, rawID uuid.UUID) models.ParsedInvoice {
	text := strings.ToUpper(rawText)
	lines := strings.Split(text, "\n")

	parsed := models.ParsedInvoice{
		ID:            uuid.New(),
		RawID:         rawID,
		AccessKey:     extractAccessKey(text),
		CNPJ:          extractCNPJ(text),
		Establishment: extractStoreName(lines),
		Date:          extractInvoiceDate(text),
		Items:         parseItems(lines),
		Total:         extractTotal(text),
		ParserVersion: "1.0.0",
	}

	parsed.Validate()

	return parsed
}

func extractAccessKey(text string) *string {
	match := reAccessKey.FindStringSubmatch(text)
	if len(match) > 1 {
		cleanKey := strings.ReplaceAll(match[1], " ", "")
		return &cleanKey
	}
	return nil
}

func extractStoreName(lines []string) *string {
	for i := 0; i < len(lines) && i < 3; i++ {
		line := strings.TrimSpace(lines[i])
		if line == "" {
			continue
		}

		if reCNPJ.MatchString(line) {
			nameOnly := reCNPJ.ReplaceAllString(line, "")
			nameOnly = strings.ReplaceAll(nameOnly, "CNPJ:", "")
			nameOnly = strings.TrimSpace(nameOnly)

			if len(nameOnly) > 3 {
				return &nameOnly
			}
			continue
		}
		return &line
	}
	return nil
}

func extractCNPJ(text string) *string {
	match := reCNPJ.FindString(text)
	if match == "" {
		return nil
	}
	return &match
}

func extractInvoiceDate(text string) *time.Time {
	match := reDate.FindString(text)
	if match == "" {
		return nil
	}

	cleanDate := strings.ReplaceAll(match, " ", "")
	layout := "02/01/2006"

	t, err := time.Parse(layout, cleanDate)
	if err != nil {
		return nil
	}
	return &t
}

func parseItems(lines []string) []models.ParsedItem {
	var items []models.ParsedItem

	for i, line := range lines {
		matches := reItemLine.FindStringSubmatch(line)
		if len(matches) > 0 {
			item := models.ParsedItem{}

			if i > 0 {
				name := strings.TrimSpace(lines[i-1])
				item.Name = &name
			}

			items = append(items, item)
		}
	}
	return items
}

func extractTotal(text string) *float64 {
	match := reTotalAmount.FindStringSubmatch(text)

	if len(match) > 1 {
		cleanTotal := strings.ReplaceAll(match[1], " ", "")
		cleanTotal = strings.ReplaceAll(cleanTotal, ",", ".")

		value, err := strconv.ParseFloat(cleanTotal, 64)
		if err != nil {
			return nil
		}
		return &value
	}
	return nil
}
