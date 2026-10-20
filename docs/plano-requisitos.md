# Plano de Requisitos: Site de Casamento (versão 1.0)

**Casal:** Rafael Takaki e Thatiany Freitas
**Data:** 26/06/2027 (sábado)
**Convidados:** ~150
**Estilo:** boho chic, com marsala suavizado para tons pastel

## 1. Local e informações do evento

- **Cerimônia:** Basílica Menor de Santo Antônio do Embaré (Igreja do Embaré), Avenida Bartolomeu de Gusmão, 32, Embaré, Santos/SP. Telefone: (13) 3227-5977. Confirmar com a paróquia a disponibilidade e as regras da cerimônia antes de divulgar o horário.
- **Festa:** AABB Santos (Associação Atlética Banco do Brasil), Avenida Ana Costa, 442, Gonzaga, Santos/SP, CEP 11060-310. Telefone: (13) 3289-4334. E-mail: comunicacao@aabbsantos.com.br. O salão social comporta até 460 pessoas.

**Observação:** o site do clube também cita uma barraca de praia (Vicente de Carvalho, 74), que não é a sede da festa. O CEP aparece com dois valores (11060-310 e 11060-002). Confirmar com o clube antes de publicar no mapa.

## 2. Perfis de uso

- **Visitante público:** acessa informações gerais, sem área restrita.
- **Convidado:** sem login. Confirma presença buscando o próprio nome.
- **Noivos e acessora:** acesso administrativo com login.

## 3. Páginas públicas

- **RF-01 Início:** nomes do casal, data, contagem regressiva até 26/06/2027 e foto de destaque.
- **RF-02 História do casal:** texto e fotos.
- **RF-03 Cerimônia e festa:** local, mapa, horários, dress code e orientações de chegada para os dois endereços.
- **RF-04 Programação:** cronograma do dia.
- **RF-05 Galeria:** fotos dos noivos, sem upload.
- **RF-06 Lista de presentes:** itens com status (disponível ou presenteado).
- **RF-07 Chave PIX:** exibida com botão de copiar.
- **RF-08 Perguntas frequentes e contato:** WhatsApp dos noivos e da acessora.

## 4. Confirmação de presença

**Busca**

- **RF-10:** o convidado digita o nome completo corretamente. A comparação ignora acentos e diferenças de maiúsculas e minúsculas.
- **RF-11:** a busca não mostra sugestões nem lista de nomes.
- **RF-12:** se o nome não for encontrado, o site exibe: "Não encontramos seu nome. Entre em contato com os noivos ou com a acessora", com links de WhatsApp.
- **RF-13:** homônimos são diferenciados no cadastro (sobrenome ou grupo), não na busca.

**Resposta**

- **RF-20:** após encontrar o nome, o site mostra o convidado principal e a lista fixa de acompanhantes cadastrados.
- **RF-21:** cada pessoa tem três opções por checkbox: **Vou**, **Não vou** e **A confirmar**. "A confirmar" é o estado inicial.
- **RF-22:** a confirmação pode ser parcial. O convidado principal pode confirmar só a si mesmo e voltar depois para confirmar os acompanhantes.
- **RF-23:** o convidado pode voltar quantas vezes quiser até o prazo, enquanto houver pessoas em "A confirmar".
- **RF-24:** exceção: alguns convites permitem cadastrar um acompanhante com nome livre. Essa pessoa entra direto na lista, sem aprovação. O admin define, convite a convite, se a opção existe e quantas pessoas podem ser adicionadas.

**Irreversibilidade**

- **RF-30:** após marcar "Vou" ou "Não vou", a resposta não pode ser alterada pelo site. Vale para o principal e para cada acompanhante.
- **RF-31:** alterações são feitas pelos noivos ou pela acessora, pelo painel administrativo.
- **RF-32:** só "A confirmar" pode mudar. Não existe cancelamento pelo site.

**Prazo**

- **RF-40:** o prazo termina em **06/06/2027 às 23h59** (horário de Brasília). A data e o horário ficam em variável de ambiente, para que o prazo possa ser estendido sem alteração de código.
- **RF-41:** após o prazo, o site bloqueia novas confirmações e mostra a mensagem de contato com os noivos. As respostas já registradas continuam visíveis.

## 5. Fotos e recados

- **RF-50 Upload:** o convidado envia fotos informando o próprio nome. As fotos são publicadas automaticamente no álbum colaborativo.
- **RF-51 Moderação:** o admin pode remover qualquer foto.
- **RF-52 Limites:** tamanho máximo, formatos aceitos (JPG, PNG, HEIC) e quantidade por envio, a definir na fase técnica.
- **RF-53 Recados:** o convidado escreve uma mensagem informando o nome. Os recados são visíveis somente aos noivos, no painel administrativo.

## 6. Lista de presentes

- **RF-60:** cada item tem nome, imagem, descrição, preço (opcional) e status.
- **RF-61:** ao escolher um item, o convidado informa o próprio nome. O item fica como "presenteado".
- **RF-62:** o sistema impede que dois convidados reservem o mesmo item ao mesmo tempo.
- **RF-63:** o admin pode liberar um item, caso o presente não seja enviado.

## 7. Administração

- **RF-70:** cadastro de convidados com nome, grupo, telefone (WhatsApp), acompanhantes fixos e limite de acompanhantes livres.
- **RF-71:** painel de confirmações com totais de "Vou", "Não vou" e "A confirmar".
- **RF-72:** exportação em CSV e PDF para buffet, cerimonial e fornecedores.
- **RF-73:** moderação de fotos e leitura de recados.
- **RF-74:** ajuste de respostas a pedido do convidado.
- **RF-75:** gestão de presentes e da programação.

## 8. Lembretes automáticos (WhatsApp)

- **RF-80:** mensagens apenas para convidados com pelo menos uma pessoa em "A confirmar".
- **RF-81:** a cada 14 dias, a partir de 26/03/2027 (três meses antes do casamento), até o prazo final. Datas previstas: 26/03, 09/04, 23/04, 07/05, 21/05 e 04/06.
- **RF-82:** o envio exige templates aprovados pela Meta e tem custo por mensagem, que deve entrar no orçamento.

## 9. Privacidade

- **RF-90:** o site não é indexado por buscadores (`noindex`).
- **RF-91:** dados de convidados (nome, telefone e restrições) não aparecem em páginas públicas.
- **RF-92:** o upload de fotos tem limite de envios por período para evitar spam.

## 10. Diretrizes de estilo

- **Conceito:** boho chic, com folhagens secas, arcos, pampas, linho, papel kraft e texturas naturais.
- **Paleta sugerida:**
  - Marsala pastel principal (#B5858C), para títulos e destaques.
  - Marsala profundo (#7A4A52), para botões e textos pequenos.
  - Off-white quente (#FBF7F2), como fundo principal.
  - Bege areia (#E9DDCD), para seções alternadas.
  - Verde-oliva suave (#9AA48A), como apoio nas ilustrações.
- **Tipografia sugerida:** Cormorant Garamond para títulos e Jost ou Lato para o texto.
- **Mobile first:** a maioria dos convidados acessará pelo celular, muitas vezes pelo link do WhatsApp.
- **Acessibilidade:** contraste mínimo de 4,5:1, botões grandes e textos legíveis.
- **Animações:** discretas, como fade ao rolar a página.

## 11. Pendências

1. Confirmar com a paróquia a disponibilidade e o horário da cerimônia.
2. Confirmar com o AABB Santos o CEP e as condições de locação do salão.
3. Definir o tamanho máximo e a quantidade de fotos por envio (RF-52).
4. Definir o texto oficial das mensagens de WhatsApp para aprovação na Meta.
