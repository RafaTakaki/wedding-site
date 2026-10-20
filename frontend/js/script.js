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

  if (page === 'admin') {
    initAdminPage();
  }
});

function apiBaseUrl() {
  return (document.body.dataset.apiBase || 'http://localhost:5000').replace(/\/$/, '');
}

async function readApiError(response) {
  try {
    const body = await response.json();
    return body.message || body.detail || 'Não foi possível concluir a operação.';
  } catch (error) {
    return 'Não foi possível conectar ao servidor. Confira se a API está em execução.';
  }
}

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

function initConfirmacaoPage() {
  const input = document.getElementById('nome');
  const buscar = document.getElementById('buscarConvite');
  const resultado = document.getElementById('resultado');
  const lista = document.getElementById('listaConvidados');
  const salvar = document.getElementById('salvarConfirmacao');
  const mensagem = document.getElementById('mensagemStatus');

  if (!input || !buscar || !resultado || !lista || !salvar || !mensagem) {
    return;
  }

  let conviteAtual = null;

  const resetMensagem = () => {
    mensagem.textContent = '';
    mensagem.classList.remove('success', 'error');
  };

  const montarLista = (registro) => {
    lista.replaceChildren();
    registro.people.forEach((person) => {
      const card = document.createElement('div');
      card.className = 'pessoa';
      const name = document.createElement('div');
      name.className = 'nome-pessoa';
      name.textContent = person.name;
      const options = document.createElement('div');
      options.className = 'opcoes';
      options.setAttribute('role', 'radiogroup');
      options.setAttribute('aria-label', `Status de ${person.name}`);

      [
        ['vou', 'Vou'],
        ['nao-vou', 'Não vou'],
        ['a-confirmar', 'A confirmar']
      ].forEach(([value, labelText]) => {
        const label = document.createElement('label');
        const radio = document.createElement('input');
        radio.type = 'radio';
        radio.name = `status-${person.id}`;
        radio.value = value;
        radio.checked = person.status === value;
        radio.disabled = !registro.isRsvpOpen || person.status !== 'a-confirmar';
        label.append(radio, document.createTextNode(labelText));
        options.append(label);
      });

      card.append(name, options);
      lista.append(card);
    });

    salvar.disabled = !registro.isRsvpOpen || !registro.people.some((person) => person.status === 'a-confirmar');
    if (!registro.isRsvpOpen) {
      mensagem.textContent = 'O prazo para confirmação terminou. Entre em contato com os noivos.';
      mensagem.classList.add('visible', 'error');
    }
  };

  const atualizarResultado = (registro) => {
    resultado.replaceChildren();
    resultado.classList.add('visible');
    const title = document.createElement('strong');
    title.textContent = `Convite encontrado: ${registro.name}`;
    const detail = document.createElement('small');
    detail.textContent = 'Confirme cada pessoa individualmente antes do prazo.';
    resultado.append(title, document.createElement('br'), detail);
  };

  buscar.addEventListener('click', async () => {
    resetMensagem();
    const valor = input.value.trim();
    lista.replaceChildren();
    conviteAtual = null;

    if (!valor) {
      resultado.classList.add('visible');
      resultado.textContent = 'Digite seu nome completo para continuar.';
      return;
    }

    buscar.disabled = true;
    try {
      const response = await fetch(`${apiBaseUrl()}/api/guests/lookup?name=${encodeURIComponent(valor)}`);
      if (!response.ok) throw new Error(await readApiError(response));
      conviteAtual = await response.json();
      atualizarResultado(conviteAtual);
      montarLista(conviteAtual);
    } catch (error) {
      resultado.classList.add('visible');
      resultado.textContent = error.message;
    } finally {
      buscar.disabled = false;
    }
  });

  salvar.addEventListener('click', async () => {
    if (!conviteAtual) return;
    const answers = conviteAtual.people
      .filter((person) => person.status === 'a-confirmar')
      .map((person) => ({
        inviteeId: person.id,
        status: lista.querySelector(`input[name="status-${person.id}"]:checked`)?.value || person.status
      }));

    if (!answers.some((answer) => answer.status !== 'a-confirmar')) {
      mensagem.textContent = 'Escolha Vou ou Não vou para pelo menos uma pessoa.';
      mensagem.classList.add('visible', 'error');
      return;
    }

    salvar.disabled = true;
    try {
      const response = await fetch(`${apiBaseUrl()}/api/guests/rsvp/${encodeURIComponent(conviteAtual.rsvpToken)}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ answers })
      });
      if (!response.ok) throw new Error(await readApiError(response));
      answers.forEach((answer) => {
        const person = conviteAtual.people.find((item) => item.id === answer.inviteeId);
        if (person) person.status = answer.status;
      });
      montarLista(conviteAtual);
      mensagem.textContent = 'Confirmação salva. Respostas enviadas não podem ser alteradas pelo site.';
      mensagem.classList.add('visible', 'success');
    } catch (error) {
      mensagem.textContent = error.message;
      mensagem.classList.add('visible', 'error');
      salvar.disabled = false;
    }
  });
}

function initAdminPage() {
  const connectForm = document.getElementById('adminConnectForm');
  const keyInput = document.getElementById('adminKey');
  const createForm = document.getElementById('guestCreateForm');
  const message = document.getElementById('adminMessage');
  const guestList = document.getElementById('guestList');
  let adminKey = '';

  const adminFetch = (path, options = {}) => fetch(`${apiBaseUrl()}${path}`, {
    ...options,
    headers: {
      ...options.headers,
      'X-Admin-Key': adminKey
    }
  });

  const loadGuests = async () => {
    const response = await adminFetch('/api/admin/guests');
    if (!response.ok) throw new Error(await readApiError(response));
    const invitations = await response.json();
    guestList.replaceChildren();

    const counts = { vou: 0, 'nao-vou': 0, 'a-confirmar': 0 };
    invitations.forEach((invitation) => {
      const row = document.createElement('article');
      row.className = 'guest-row';
      const title = document.createElement('h2');
      title.textContent = invitation.name;
      const detail = document.createElement('p');
      const people = invitation.people || [];
      people.forEach((person) => {
        counts[person.status] = (counts[person.status] || 0) + 1;
      });
      detail.textContent = [invitation.groupName, `${people.length} pessoa(s)`, invitation.whatsApp].filter(Boolean).join(' · ');
      row.append(title, detail);
      guestList.append(row);
    });

    document.getElementById('adminGoing').textContent = counts.vou;
    document.getElementById('adminNotGoing').textContent = counts['nao-vou'];
    document.getElementById('adminPending').textContent = counts['a-confirmar'];
    createForm.hidden = false;
    message.textContent = `${invitations.length} convite(s) carregado(s).`;
  };

  connectForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    adminKey = keyInput.value;
    message.textContent = 'Conectando...';
    try {
      await loadGuests();
    } catch (error) {
      adminKey = '';
      message.textContent = error.message;
    }
  });

  createForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    const form = new FormData(createForm);
    const allowFreeCompanions = form.get('allowFreeCompanions') === 'on';
    const request = {
      name: form.get('name').trim(),
      groupName: form.get('groupName').trim() || null,
      whatsApp: form.get('whatsApp').trim() || null,
      companions: form.get('companions').split(/\r?\n/).map((name) => name.trim()).filter(Boolean),
      allowFreeCompanions,
      freeCompanionLimit: allowFreeCompanions ? Number(form.get('freeCompanionLimit')) : 0
    };

    try {
      const response = await adminFetch('/api/admin/guests', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      });
      if (!response.ok) throw new Error(await readApiError(response));
      createForm.reset();
      createForm.elements.freeCompanionLimit.value = '0';
      await loadGuests();
      message.textContent = 'Convite cadastrado.';
    } catch (error) {
      message.textContent = error.message;
    }
  });
}

