// Package messaging to determine chanel comms
package messaging

import (
	"encoding/json"

	"github.com/LuisTerranova/invoices-app/backend-go/internal/models"
	amqp "github.com/rabbitmq/amqp091-go"
)

func ToRawInvoice(body []byte) (models.RawInvoice, error) {
	var raw models.RawInvoice
	err := json.Unmarshal(body, &raw)
	return raw, err
}

func PublishParsedInvoice(ch *amqp.Channel, result models.ParsedInvoice) error {
	body, err := json.Marshal(result)
	if err != nil {
		return err
	}

	return ch.Publish(
		"",                   // exchange
		"processed_invoices", // .NET return queue
		false,
		false,
		amqp.Publishing{
			ContentType: "application/json",
			Body:        body,
		},
	)
}
