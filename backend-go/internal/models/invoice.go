// Package models: accomodate structs
package models

import "time"

type RawInvoice struct {
	ID        int
	ImageURL  string
	Content   string
	CreatedAt time.Time
}

type ParsedInvoice struct {
	RawID         int
	Establishment *string
	CNPJ          *string
	Date          *time.Time
	Total         *float64
	Itens         []ParsedItem
	ParserVersion string
	IsValid       bool
	ParseError    *string
}

type Invoice struct {
	ID            int
	RawID         int
	Establishment string
	CNPJ          string
	Date          time.Time
	Total         float64
	Itens         []Item
	CreatedAt     time.Time
}

type ParsedItem struct {
	Name      *string
	Quantity  *int
	UnitPrice *float64
	Total     *float64
}

type Item struct {
	Name      *string
	Quantity  *int
	UnitPrice *float64
	Total     *float64
}
