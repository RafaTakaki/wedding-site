document.addEventListener('DOMContentLoaded', () => {
  const image = document.querySelector('.foto');

  if (image) {
    image.addEventListener('error', () => {
      image.style.display = 'none';
    });
  }

  const page = document.body.dataset.page;

  if (page === 'home') {
    initHomePage();
  }

  if (page === 'confirmacao') {
    initConfirmacaoPage();
  }

  if (page === 'presentes') {
    initPresentesPage();
  }
});

function initHomePage() {
  const targetDate = new Date('2027-06-26T18:00:00-03:00');
  const countdown = document.querySelector('[data-countdown]');

  if (!countdown) return;

  const updateCountdown = () => {
    const diff = targetDate.getTime() - Date.now();

    if (diff <= 0) {
      countdown.innerHTML = '<div class="countdown-item"><strong>Hoje</strong><span>É o dia!</span></div>';
      return;
    }

    const totalSeconds = Math.floor(diff / 1000);
    const days = Math.floor(totalSeconds / 86400);
    const hours = Math.floor((totalSeconds % 86400) / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    countdown.innerHTML = `
      <div class="countdown-item">
        <strong>${days}</strong>
        <span>dias</span>
      </div>
      <div class="countdown-item">
        <strong>${hours}</strong>
        <span>horas</span>
      </div>
      <div class="countdown-item">
        <strong>${minutes}</strong>
        <span>minutos</span>
      </div>
      <div class="countdown-item">
        <strong>${seconds}</strong>
        <span>segundos</span>
      </div>
    `;
  };

  updateCountdown();
  setInterval(updateCountdown, 1000);
}

function initPresentesPage() {
  const copyButtons = document.querySelectorAll('[data-copy-pix]');
  const reserveButtons = document.querySelectorAll('[data-reserve-item]');

  copyButtons.forEach((button) => {
    button.addEventListener('click', async () => {
      const value = button.dataset.copyPix;

      try {
        await navigator.clipboard.writeText(value);
        button.textContent = 'Copiado';
      } catch (error) {
        button.textContent = 'Copiar';
      }

      window.setTimeout(() => {
        button.textContent = 'Copiar chave';
      }, 1500);
    });
  });

  reserveButtons.forEach((button) => {
    button.addEventListener('click', () => {
      const card = button.closest('.gift-card');
      const status = card?.querySelector('.gift-status');

      if (!status) return;

      status.textContent = 'Reservado';
      status.classList.add('reserved');
      button.textContent = 'Reservado';
      button.disabled = true;
    });
  });
}

function normalizarNome(valor) {
  return (valor || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/\s+/g, ' ')
    .trim()
    .toLowerCase();
}

function initConfirmacaoPage() {
  const convidados = [
    {
      id: 'rafael-takaki',
      nome: 'Rafael Takaki',
      status: 'a-confirmar',
      acompanhantes: [
        { id: 'thatiany-freitas', nome: 'Thatiany Freitas', status: 'a-confirmar' }
      ]
    },
    {
      id: 'maria-da-silva',
      nome: 'Maria da Silva',
      status: 'a-confirmar',
      acompanhantes: [
        { id: 'joao-da-silva', nome: 'João da Silva', status: 'a-confirmar' },
        { id: 'ana-da-silva', nome: 'Ana da Silva', status: 'a-confirmar' }
      ]
    }
  ];

  const input = document.getElementById('nome');
  const buscar = document.getElementById('buscarConvite');
  const resultado = document.getElementById('resultado');
  const lista = document.getElementById('listaConvidados');
  const salvar = document.getElementById('salvarConfirmacao');
  const mensagem = document.getElementById('mensagemStatus');

  if (!input || !buscar || !resultado || !lista || !salvar || !mensagem) {
    return;
  }

  const resetMensagem = () => {
    mensagem.textContent = '';
    mensagem.classList.remove('success', 'error');
  };

  const renderPessoa = (pessoa) => {
    const statusGrupo = `status-${pessoa.id}`;

    return `
      <div class="pessoa">
        <div class="nome-pessoa">${pessoa.nome}</div>
        <div class="opcoes" role="radiogroup" aria-label="Status de ${pessoa.nome}">
          <label>
            <input type="radio" name="${statusGrupo}" value="vou" ${pessoa.status === 'vou' ? 'checked' : ''}>
            <span>Vou</span>
          </label>
          <label>
            <input type="radio" name="${statusGrupo}" value="nao-vou" ${pessoa.status === 'nao-vou' ? 'checked' : ''}>
            <span>Não vou</span>
          </label>
          <label>
            <input type="radio" name="${statusGrupo}" value="a-confirmar" ${pessoa.status === 'a-confirmar' ? 'checked' : ''}>
            <span>A confirmar</span>
          </label>
        </div>
      </div>
    `;
  };

  const montarLista = (registro) => {
    const pessoas = [
      { id: registro.id, nome: registro.nome, status: registro.status },
      ...registro.acompanhantes.map((acompanhante) => ({
        id: acompanhante.id,
        nome: acompanhante.nome,
        status: acompanhante.status
      }))
    ];

    lista.innerHTML = pessoas.map(renderPessoa).join('');

    lista.querySelectorAll('input[type="radio"]').forEach((radio) => {
      radio.addEventListener('change', () => {
        const pessoa = pessoas.find((item) => `status-${item.id}` === radio.name);
        if (pessoa) {
          pessoa.status = radio.value;
        }
      });
    });
  };

  const atualizarResultado = (registro) => {
    resultado.classList.add('visible');
    resultado.innerHTML = `
      <strong>Convidado encontrado:</strong> ${registro.nome}<br>
      <small>Você pode confirmar cada pessoa individualmente antes do prazo.</small>
    `;
  };

  buscar.addEventListener('click', () => {
    resetMensagem();
    const valor = input.value.trim();

    if (!valor) {
      resultado.classList.add('visible');
      resultado.innerHTML = 'Digite seu nome completo para continuar.';
      lista.innerHTML = '';
      return;
    }

    const registro = convidados.find((item) => normalizarNome(item.nome) === normalizarNome(valor));

    if (!registro) {
      resultado.classList.add('visible');
      resultado.innerHTML = 'Não encontramos seu nome. Entre em contato com os noivos ou com a acessora.';
      lista.innerHTML = '';
      return;
    }

    atualizarResultado(registro);
    montarLista(registro);
  });

  salvar.addEventListener('click', () => {
    const radios = document.querySelectorAll('input[type="radio"]:checked');
    const total = radios.length;

    if (!total) {
      mensagem.textContent = 'Selecione ao menos uma resposta para salvar.';
      mensagem.classList.add('error');
      return;
    }

    const resumo = Array.from(radios).map((radio) => `${radio.name.replace('status-', '')}: ${radio.value}`);

    mensagem.textContent = `Confirmação salva com sucesso. ${resumo.length} pessoa(s) atualizadas.`;
    mensagem.classList.add('success');
  });
}

