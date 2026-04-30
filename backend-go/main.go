package main

import (
	"log"

	"github.com/LuisTerranova/invoices-app/backend-go/internal/messaging"
	"github.com/LuisTerranova/invoices-app/backend-go/internal/models"
	"github.com/LuisTerranova/invoices-app/backend-go/internal/ocr"
	"github.com/LuisTerranova/invoices-app/backend-go/internal/parser"
	amqp "github.com/rabbitmq/amqp091-go"
)

func main() {
	conn, err := amqp.Dial("ampq://guest:guest@localhost:5672")
	if err != nil {
		log.Fatal(err)
	}

	ch, err := conn.Channel()
	if err != nil {
		log.Fatal(err)
	}
	defer conn.Close()

	q, _ := ch.QueueDeclare("invoices_to_process", true, false, false, false, nil)

	msgs, _ := ch.Consume(q.Name, "", false, false, false, false, nil)

	log.Println("[*] Awaiting invoices. Press CTRL+C to stop process")

	for d := range msgs {
		raw, err := messaging.ToRawInvoice(d.Body)
		if err != nil {
			log.Printf("Error: %v", err)
			continue
		}

		go func(input models.RawInvoice, delivery amqp.Delivery) {
			extractedText, err := ocr.ExtractText(input.ImageData)

			result := parser.Parse(extractedText, raw.ID, err)

			messaging.PublishParsedInvoice(ch, result)

			delivery.Ack(false)
		}(raw, d)
	}
}
