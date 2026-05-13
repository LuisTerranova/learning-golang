// Package parser: parser definition
package parser

import (
	"regexp"
	"strconv"
	"strings"
	"time"

	"github.com/google/uuid"

	"github.com/LuisTerranova/invoices-app/backend-go/internal/models"
)

func Parse(rawText string, rawID uuid.UUID) models.ParsedInvoice {
	text := cleanOCRText(rawText)
	text = strings.ToUpper(text)
	lines := strings.Split(text, "\n")

	parsed := models.ParsedInvoice{
		ID:            uuid.New(),
		RawID:         rawID,
		RawText:       &text,
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
		cleanKey := strings.TrimSpace(strings.ReplaceAll(match[1], " ", ""))
		return &cleanKey
	}
	return nil
}

func extractStoreName(lines []string) *string {
	var earliest string

	for i := 0; i < len(lines) && i < 10; i++ {
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

		if earliest == "" {
			earliest = line
		}

		// Prefer short-to-medium lines in first 5 (store names are rarely long)
		if i < 5 && len(line) < 80 && !strings.Contains(line, "NFE") && !strings.Contains(line, "NFC-E") {
			storeName := strings.TrimSpace(line)
			return &storeName
		}
	}

	if earliest != "" {
		return &earliest
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
	cleanDate = strings.ReplaceAll(cleanDate, "-", "/")
	cleanDate = strings.ReplaceAll(cleanDate, ".", "/")

	layout := "02/01/2006"
	t, err := time.Parse(layout, cleanDate)
	if err != nil {
		return nil
	}
	return &t
}

func isBarcode(s string) bool {
	return regexp.MustCompile(`\d{8,}`).MatchString(s)
}

func cleanOCRText(text string) string {
	// Collapse horizontal whitespace only — preserve newlines
	re := regexp.MustCompile(`[ \t]+`)
	text = re.ReplaceAllString(text, " ")

	// Trim each line
	var builder strings.Builder
	for _, line := range strings.Split(text, "\n") {
		if builder.Len() > 0 {
			builder.WriteByte('\n')
		}
		trimmed := strings.TrimSpace(line)
		// Skip empty trailing lines
		if trimmed == "" && builder.Len() == 0 {
			continue
		}
		builder.WriteString(trimmed)
	}

	return builder.String()
}

func parseItems(lines []string) []models.ParsedItem {
	var items []models.ParsedItem

	for i, line := range lines {
		for _, re := range itemPatterns {
			matches := re.FindStringSubmatch(line)
			if len(matches) == 0 {
				continue
			}

			item := models.ParsedItem{}
			groupNames := re.SubexpNames()

			// Try same-line text before the match as item name
			matchPos := strings.Index(line, matches[0])
			if matchPos > 0 {
				name := strings.TrimSpace(line[:matchPos])
				if len(name) > 0 && !isBarcode(name) {
					item.Name = &name
				}
			}

			// Fall back to previous line
			if item.Name == nil && i > 0 {
				name := strings.TrimSpace(lines[i-1])
				if len(name) > 0 && !isBarcode(name) {
					item.Name = &name
				}
			}

			for idx, name := range groupNames {
				if idx == 0 || idx >= len(matches) {
					continue
				}

				switch name {
				case "qty":
					if matches[idx] != "" {
						if v := parseBrazilianFloat(matches[idx]); v != nil {
							q := int(*v)
							item.Quantity = &q
						}
					} else if item.Quantity == nil {
						q := 1
						item.Quantity = &q
					}
				case "unit_price":
					item.UnitPrice = parseBrazilianFloat(matches[idx])
				case "total_item":
					item.Total = parseBrazilianFloat(matches[idx])
				case "total_item_raw":
					item.Total = extractLastNumber(matches[idx])
				}
			}

			items = append(items, item)
			break // first matching pattern wins for this line
		}
	}
	return items
}

func parseBrazilianFloat(s string) *float64 {
	clean := strings.ReplaceAll(s, " ", "")
	clean = strings.ReplaceAll(clean, ".", "")
	clean = strings.ReplaceAll(clean, ",", ".")
	value, err := strconv.ParseFloat(clean, 64)
	if err != nil {
		return nil
	}
	return &value
}

func extractLastNumber(s string) *float64 {
	re := regexp.MustCompile(`[\d,.]+`)
	matches := re.FindAllString(s, -1)
	for i := len(matches) - 1; i >= 0; i-- {
		if v := parseBrazilianFloat(matches[i]); v != nil {
			return v
		}
	}
	return nil
}

func extractTotal(text string) *float64 {
	match := reTotalAmount.FindStringSubmatch(text)
	if len(match) > 1 {
		return parseBrazilianFloat(match[1])
	}
	return nil
}
