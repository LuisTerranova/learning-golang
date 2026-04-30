# 📑 Invoices App: OCR & Inteligência Fiscal

Este projeto é uma implementação de arquitetura de microserviços focada na extração automatizada de dados de Notas Fiscais (NFC-e). O objetivo central é demonstrar a integração entre tecnologias distintas, aproveitando o que cada ecossistema oferece de melhor para resolver um problema de automação fiscal.

## 🎯 Por que essa Stack?

A escolha das tecnologias foi estratégica para unir produtividade, performance e especialização:

* **Go (Backend):** Escolhido pela agilidade de desenvolvimento e pela concorrência nativa com *goroutines*. Isso permite que o processamento de imagens e as chamadas ao motor de OCR sejam executados de forma leve e altamente eficiente no lado do servidor.
* **.NET / Avalonia (Frontend):** Utiliza a robustez e a vasta biblioteca de classes do ecossistema .NET para criar uma interface desktop moderna, estável e verdadeiramente multiplataforma, mantendo a familiaridade do C#.
* Mensageria (RabbitMQ): O coração da comunicação entre os serviços. Em vez de chamadas diretas, o sistema utiliza um broker de mensagens para gerenciar a fila de processamento de OCR. Isso garante que o backend Go não seja sobrecarregado, permitindo o processamento assíncrono, maior resiliência a falhas e a capacidade de escalar o worker de OCR independentemente da interface.

## 🏗️ Arquitetura do Sistema

O sistema segue um modelo de comunicação eficiente entre um cliente desktop e um serviço especializado de processamento:



1.  **Serviço de OCR (Go):** * Atua como o servidor gRPC.
    * Recebe o stream de bytes da imagem.
    * Utiliza o **Tesseract OCR** para a extração bruta de texto.
    * Aplica filtros de **Regex** de alta precisão para estruturar os dados essenciais (CNPJ, Data, Valor Total).
2.  **Cliente Desktop (Avalonia):** * Gerencia o fluxo de trabalho do usuário final.
    * Permite o upload de arquivos e a visualização em tempo real do status de processamento.
    * Facilita a validação visual dos dados processados através do padrão MVVM.

## 🛠️ Stack Tecnológica

### Backend (Processamento)
* **Linguagem:** Go (Golang)
* **OCR Engine:** Tesseract OCR
* **Mensageria:** RabbitMQ

### Frontend (Interface)
* **Linguagem:** C# (.NET 10)
* **UI Framework:** Avalonia UI (Multiplataforma)
* **Padrão de Arquitetura:** MVVM (Model-View-ViewModel)

---
*Projeto desenvolvido para uso pessoal e fins de estudo em arquitetura de sistemas e processamento distribuído.*
