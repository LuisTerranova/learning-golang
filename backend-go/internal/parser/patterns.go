// package parser: parser definition
package parser

import "regexp"

var (

	// Capture 44-digit access key (handles OCR spaces)
	reAccessKey = regexp.MustCompile(`(?i)(?:CHAVE|CONSULTA)\D*(\d[\s\d]{43,60})`)

	// Match standard Brazilian CNPJ format [cite: 1, 20]
	reCNPJ = regexp.MustCompile(`\d{2}\.?\d{3}\.?\d{3}/?\d{4}-?\d{2}`)

	// Extract date in DD/MM/YYYY format [cite: 9, 10]
	reDate = regexp.MustCompile(`\d{2}/\d{2}/\d{4}`)

	// Match final amount due (Valor a Pagar)
	reTotalAmount = regexp.MustCompile(`(?i)(?:VALOR\s+A\s+PAGAR|TOTAL)\s*R?\$?\s*([\d\s,.]+)`)

	// Extract receipt items: [Qty] [Unit] [Unit Price] [Line Total]
	// Example match: "1,000 UN 10 99"
	reItemLine = regexp.MustCompile(`(?P<qty>[\d,.]+)\s+(?P<unit>UN|KG|L|UNID)\s*(?:X\s*)?(?P<unit_price>[\d\s,.]+)\s+(?P<total_item>[\d\s,.]+)`)

	// Identify store names/headers
	reStoreName = regexp.MustCompile(`(?i)(?:LOJA|DISTRIBUIDORA|MERCADO|FARMACIA|POSTO).*`)
)
