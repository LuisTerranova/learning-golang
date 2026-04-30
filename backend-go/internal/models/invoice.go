// package models to accomodate models
package models

import (
	"time"

	"github.com/google/uuid"
)

type RawInvoice struct {
	ID        uuid.UUID
	Content   []byte
	CreatedAt time.Time
}

type ParsedInvoice struct {
	ID            uuid.UUID
	RawID         uuid.UUID
	RawText       string
	AccessKey     *string
	Establishment *string
	CNPJ          *string
	Date          *time.Time
	Total         *float64
	Items         []ParsedItem
	ParserVersion string
	IsValid       bool
	ParseError    *string
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
