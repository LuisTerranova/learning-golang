package main

import (
	"log"
	"os"
	"os/signal"
	"sync"
	"syscall"

	"github.com/LuisTerranova/invoices-app/backend-go/internal/messaging"
	"github.com/LuisTerranova/invoices-app/backend-go/internal/ocr"
	"github.com/LuisTerranova/invoices-app/backend-go/internal/parser"
	amqp "github.com/rabbitmq/amqp091-go"
)

func main() {
	conn, err := amqp.Dial("amqp://guest:guest@localhost:5672")
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

	sigCh := make(chan os.Signal, 1)
	signal.Notify(sigCh, syscall.SIGINT, syscall.SIGTERM)

	go func() {
		<-sigCh
		log.Println("Shutting down...")
		ch.Cancel(q.Name, false)
	}()

	sem := make(chan struct{}, 10)
	var wg sync.WaitGroup

	for d := range msgs {
		sem <- struct{}{}

		wg.Add(1)
		go func(delivery amqp.Delivery) {
			defer wg.Done()
			defer func() { <-sem }()

			raw, err := messaging.ToRawInvoice(delivery.Body)
			if err != nil {
				log.Printf("Error unmarshaling invoice: %v", err)
				delivery.Nack(false, true)
				return
			}

			extractedText, ocrErr := ocr.ExtractText(raw.ImageData)
			if ocrErr != nil {
				log.Printf("OCR error: %v", ocrErr)
				delivery.Nack(false, true)
				return
			}

			result := parser.Parse(extractedText, raw.ID)

			messaging.PublishParsedInvoice(ch, result)

			delivery.Ack(false)
		}(d)
	}

	wg.Wait()
	log.Println("All workers finished. Exiting.")
}
