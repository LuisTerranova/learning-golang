// package models to accomodate models
package models

import (
	"time"

	"github.com/google/uuid"
)

type RawInvoice struct {
	ID        uuid.UUID `json:"id"`
	ImageData []byte    `json:"image_data"`
	CreatedAt time.Time `json:"created_at"`
}

type ParsedInvoice struct {
	ID            uuid.UUID    `json:"id"`
	RawID         uuid.UUID    `json:"raw_id"`
	RawText       *string      `json:"raw_text"`
	AccessKey     *string      `json:"access_key"`
	Establishment *string      `json:"establishment"`
	CNPJ          *string      `json:"cnpj"`
	Date          *time.Time   `json:"date"`
	Total         *float64     `json:"total"`
	Items         []ParsedItem `json:"items"`
	ParserVersion string       `json:"parser_version"`
	IsValid       bool         `json:"is_valid"`
	ParseErrors   []string     `json:"parse_errors"`
}

type ParsedItem struct {
	Name      *string  `json:"name"`
	Quantity  *int     `json:"quantity"`
	UnitPrice *float64 `json:"unit_price"`
	Total     *float64 `json:"total"`
}

func (p *ParsedInvoice) Validate() {
	p.IsValid = true
	p.ParseErrors = nil

	if p.CNPJ == nil {
		p.IsValid = false
		p.ParseErrors = append(p.ParseErrors, "CNPJ not identified")
	}

	if len(p.Items) == 0 {
		p.IsValid = false
		p.ParseErrors = append(p.ParseErrors, "Invoice items not identified")
	}

	if p.Total == nil || *p.Total <= 0 {
		p.IsValid = false
		p.ParseErrors = append(p.ParseErrors, "Total price not identified")
	}

	if p.Date == nil {
		p.IsValid = false
		p.ParseErrors = append(p.ParseErrors, "Invoice date not identified")
	}
}
