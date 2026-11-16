-- DB_Delivery - Projeto Integrador C# (MySQL 8+)
-- ATENÇÃO: este script APAGA e recria o banco. Rode uma vez; rodar de novo zera os dados.

DROP DATABASE IF EXISTS DB_Delivery;
CREATE DATABASE DB_Delivery
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
USE DB_Delivery;

-- ---------------------------------------------------------
-- USUÁRIOS DO SISTEMA (login do app C#)
-- ---------------------------------------------------------
CREATE TABLE usuarios (
    id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nome        VARCHAR(100) NOT NULL,
    email       VARCHAR(150) NOT NULL UNIQUE,
    senha_hash  CHAR(64)     NOT NULL,          -- SHA2-256 em hexadecimal
    criado_em   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ---------------------------------------------------------
-- PRODUTOS (cadastrados manualmente aqui; o app C# só lista/altera/exclui)
-- ---------------------------------------------------------
CREATE TABLE categorias (
    id    INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    slug  VARCHAR(30) NOT NULL UNIQUE,
    nome  VARCHAR(60) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE produtos (
    id            INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    categoria_id  INT UNSIGNED NOT NULL,
    codigo        VARCHAR(80)  NOT NULL UNIQUE,  -- mesmo "id" que o site usa no carrinho/JSON
    nome          VARCHAR(100) NOT NULL,
    descricao     VARCHAR(255) NOT NULL,
    preco         DECIMAL(8,2) NOT NULL CHECK (preco >= 0),
    imagem        VARCHAR(255),
    ativo         BOOLEAN      NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_produtos_categoria
        FOREIGN KEY (categoria_id) REFERENCES categorias(id)
) ENGINE=InnoDB;

-- ---------------------------------------------------------
-- CLIENTES e ENTREGADORES (candidatos aos 2 CRUDs extras)
-- ---------------------------------------------------------
CREATE TABLE clientes (
    id        INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nome      VARCHAR(100) NOT NULL,
    email     VARCHAR(150) UNIQUE,
    telefone  VARCHAR(20),
    endereco  VARCHAR(255)
) ENGINE=InnoDB;

CREATE TABLE entregadores (
    id        INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nome      VARCHAR(100) NOT NULL,
    telefone  VARCHAR(20)  NOT NULL,
    veiculo   VARCHAR(40),
    placa     VARCHAR(10),
    ativo     BOOLEAN NOT NULL DEFAULT TRUE
) ENGINE=InnoDB;

-- ---------------------------------------------------------
-- PEDIDOS (entram só pela importação do JSON do site)
-- ---------------------------------------------------------
CREATE TABLE pedidos (
    id               INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    id_externo       VARCHAR(40)  NOT NULL UNIQUE,   -- id do pedido no JSON; evita importar duas vezes
    cliente_id       INT UNSIGNED NULL,
    entregador_id    INT UNSIGNED NULL,
    nome_cliente     VARCHAR(100) NOT NULL,          -- cópia dos dados que vieram do site
    telefone         VARCHAR(20),
    endereco_entrega VARCHAR(255),
    status           ENUM('pendente','preparando','saiu_para_entrega','entregue','cancelado')
                     NOT NULL DEFAULT 'pendente',
    total            DECIMAL(10,2) NOT NULL DEFAULT 0,
    criado_em        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_pedidos_cliente
        FOREIGN KEY (cliente_id)    REFERENCES clientes(id)     ON DELETE SET NULL,
    CONSTRAINT fk_pedidos_entregador
        FOREIGN KEY (entregador_id) REFERENCES entregadores(id) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE itens_pedido (
    id              INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    pedido_id       INT UNSIGNED NOT NULL,
    produto_id      INT UNSIGNED NOT NULL,
    quantidade      INT UNSIGNED NOT NULL CHECK (quantidade > 0),
    preco_unitario  DECIMAL(8,2) NOT NULL,
    CONSTRAINT fk_itens_pedido
        FOREIGN KEY (pedido_id)  REFERENCES pedidos(id) ON DELETE CASCADE,
    CONSTRAINT fk_itens_produto
        FOREIGN KEY (produto_id) REFERENCES produtos(id),
    UNIQUE KEY uq_pedido_produto (pedido_id, produto_id)
) ENGINE=InnoDB;

CREATE TABLE pagamentos (
    id         INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    pedido_id  INT UNSIGNED NOT NULL,
    forma      ENUM('pix','cartao_credito','cartao_debito','dinheiro') NOT NULL,
    valor      DECIMAL(10,2) NOT NULL,
    status     ENUM('pendente','pago','recusado','estornado') NOT NULL DEFAULT 'pendente',
    pago_em    DATETIME NULL,
    CONSTRAINT fk_pagamentos_pedido
        FOREIGN KEY (pedido_id) REFERENCES pedidos(id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ---------------------------------------------------------
-- DADOS INICIAIS
-- ---------------------------------------------------------
-- Login de teste:  admin@delivery.com  /  admin123
INSERT INTO usuarios (nome, email, senha_hash)
VALUES ('Administrador', 'admin@delivery.com', SHA2('admin123', 256));

INSERT INTO categorias (slug, nome) VALUES
    ('entradas',   'Entradas'),
    ('principais', 'Pratos principais'),
    ('sobremesas', 'Sobremesas'),
    ('bebidas',    'Bebidas');

INSERT INTO produtos (categoria_id, codigo, nome, descricao, preco, imagem) VALUES
    (1, 'bruschetta-de-tomate', 'Bruschetta de tomate', 'Pão italiano tostado, tomate fresco, manjericão e azeite.', 28.00, 'image/comidas/Bruschetta-de-tomate.png'),
    (1, 'carpaccio-de-carne', 'Carpaccio de carne', 'Lâminas finas de filé, rúcula, parmesão e molho de mostarda.', 46.00, 'image/comidas/Carpaccio-de-carne.png'),
    (1, 'salada-serena', 'Salada Serena', 'Folhas verdes, tomate cereja, castanhas e vinagrete de mel.', 34.00, 'image/comidas/Salada-Serena.png'),
    (2, 'risoto-de-cogumelos', 'Risoto de cogumelos', 'Arroz arbóreo cremoso com cogumelos frescos e parmesão.', 68.00, 'image/comidas/Risoto-de-cogumelos.png'),
    (2, 'salmao-grelhado', 'Salmão grelhado', 'Salmão com legumes salteados e molho de limão siciliano.', 82.00, 'image/comidas/Salmão-grelhado.png'),
    (2, 'legumes-flambados-da-casa', 'Legumes flambados da casa', 'Mix de legumes salteados e flambados na frigideira.', 58.00, 'image/comidas/Legumes-flambados-da-casa.png'),
    (3, 'tiramisu', 'Tiramisù', 'Clássico italiano com café, mascarpone e cacau.', 26.00, 'image/comidas/Tiramisù.png'),
    (3, 'petit-gateau', 'Petit gâteau', 'Bolinho de chocolate quente com sorvete de creme.', 30.00, 'image/comidas/Petit-gâteau.png'),
    (3, 'panna-cotta', 'Panna cotta', 'Creme de baunilha com calda de frutas vermelhas.', 24.00, 'image/comidas/Panna-cotta.png'),
    (4, 'suco-natural', 'Suco natural', 'Laranja, limão, maracujá ou abacaxi com hortelã.', 14.00, 'image/comidas/Suco-natural.png'),
    (4, 'limonada-italiana', 'Limonada italiana', 'Limão siciliano, água com gás e xarope de baunilha.', 16.00, 'image/comidas/Limonada-italiana.png'),
    (4, 'vinho-da-casa-taca', 'Vinho da casa (taça)', 'Seleção de tinto ou branco, perguntar ao garçom.', 32.00, 'image/comidas/Vinho-da-casa-(taça).png');

INSERT INTO entregadores (nome, telefone, veiculo, placa) VALUES
    ('Carlos Souza', '(31) 99999-0001', 'Moto', 'ABC1D23'),
    ('Marina Lopes', '(31) 99999-0002', 'Bicicleta', NULL);
