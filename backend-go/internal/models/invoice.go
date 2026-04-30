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
	AccessKey     *string      `json:"acess_key"`
	Establishment *string      `json:"establishment"`
	CNPJ          *string      `json:"cnpj"`
	Date          *time.Time   `json:"date"`
	Total         *float64     `json:"total"`
	Items         []ParsedItem `json:"items"`
	ParserVersion string       `json:"parser_version"`
	IsValid       bool         `json:"is_valid"`
	ParseError    *string      `json:"parse_error"`
}

type ParsedItem struct {
	Name      *string
	Quantity  *int
	UnitPrice *float64
	Total     *float64
}

func (p *ParsedInvoice) Validate() {
	p.IsValid = true
	p.ParseError = nil

	if p.CNPJ == nil {
		p.IsValid = false
		msg := "CNPJ not identified"
		p.ParseError = &msg
		return
	}

	if len(p.Items) == 0 {
		p.IsValid = false
		msg := "Invoice items not identified"
		p.ParseError = &msg
		return
	}

	if p.Total == nil || *p.Total <= 0 {
		p.IsValid = false
		msg := "Total price not identified"
		p.ParseError = &msg
		return
	}

	if p.Date == nil {
		p.IsValid = false
		msg := "Invoice date not identified"
		p.ParseError = &msg
		return
	}
}
