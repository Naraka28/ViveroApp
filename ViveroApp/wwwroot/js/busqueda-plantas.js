// busqueda-plantas.js

let searchTimeout;
const searchInput = document.getElementById('searchInput');
const searchResults = document.getElementById('searchResults');
const btnBuscarImagen = document.getElementById('btnBuscarImagen');
const fileInput = document.getElementById('fileInput');
const modalIdentificacion = document.getElementById('modalIdentificacion');
const btnCerrarModal = document.getElementById('btnCerrarModal');
const contenidoModal = document.getElementById('contenidoModal');

// Búsqueda por texto
searchInput.addEventListener('input', (e) => {
    clearTimeout(searchTimeout);
    const query = e.target.value.trim();

    if (query.length < 2) {
        searchResults.classList.add('hidden');
        return;
    }

    searchTimeout = setTimeout(() => {
        buscarPorTexto(query);
    }, 300);
});

async function buscarPorTexto(query) {
    try {
        const response = await fetch(`/Plantas/BuscarTexto?q=${encodeURIComponent(query)}`);
        const result = await response.json();

        if (result.success && result.data.length > 0) {
            mostrarResultadosTexto(result.data);
        } else {
            mostrarSinResultados();
        }
    } catch (error) {
        console.error('Error en búsqueda:', error);
        mostrarError('Error al buscar plantas');
    }
}

function mostrarResultadosTexto(plantas) {
    searchResults.innerHTML = plantas.map(planta => `
        <a href="/Plantas/DetallePlanta/${planta.id}" 
           class="flex items-center gap-4 p-4 hover:bg-gray-50 transition-colors border-b last:border-b-0">
            <img src="${planta.imagen_Url || '/images/placeholder.jpg'}" 
                 alt="${planta.nombre}" 
                 class="w-16 h-16 object-cover rounded-lg">
            <div class="flex-1">
                <h3 class="text-lg font-semibold text-gray-800">${planta.nombre}</h3>
                <p class="text-sm text-gray-600">${planta.nombre_Cientifico || ''}</p>
                ${planta.dificultad ? `<span class="text-xs text-gray-500">${planta.dificultad}</span>` : ''}
            </div>
            <i class="ph ph-caret-right text-gray-400"></i>
        </a>
    `).join('');

    searchResults.classList.remove('hidden');
}

function mostrarSinResultados() {
    searchResults.innerHTML = `
        <div class="p-6 text-center text-gray-600">
            <i class="ph ph-magnifying-glass text-4xl mb-2"></i>
            <p>No se encontraron plantas</p>
        </div>
    `;
    searchResults.classList.remove('hidden');
}

function mostrarError(mensaje) {
    searchResults.innerHTML = `
        <div class="p-6 text-center text-red-600">
            <i class="ph ph-warning text-4xl mb-2"></i>
            <p>${mensaje}</p>
        </div>
    `;
    searchResults.classList.remove('hidden');
}

// Cerrar resultados al hacer clic fuera
document.addEventListener('click', (e) => {
    if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
        searchResults.classList.add('hidden');
    }
});

// Búsqueda por imagen
btnBuscarImagen.addEventListener('click', () => {
    fileInput.click();
});

fileInput.addEventListener('change', async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    // Validar que sea una imagen
    if (!file.type.startsWith('image/')) {
        alert('Por favor selecciona un archivo de imagen válido');
        return;
    }

    // Validar tamaño (máximo 5MB)
    if (file.size > 5 * 1024 * 1024) {
        alert('La imagen es demasiado grande. Máximo 5MB');
        return;
    }

    mostrarCargando();

    try {
        const imagenBase64 = await convertirABase64(file);
        await identificarPlanta(imagenBase64);
    } catch (error) {
        console.error('Error al procesar imagen:', error);
        mostrarErrorModal('Error al procesar la imagen');
    }

    // Limpiar input
    fileInput.value = '';
});

function convertirABase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = reject;
        reader.readAsDataURL(file);
    });
}

async function identificarPlanta(imagenBase64) {
    try {
        const response = await fetch('/Plantas/IdentificarPorImagen', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                imagenBase64: imagenBase64,
                latitude: 29.0729,
                longitude: -110.9559
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        console.log('Respuesta completa del servidor:', result); // Debug completo

        if (result.success && result.data) {
            mostrarResultadoIdentificacion(result.data);
        } else {
            // Mostrar el mensaje específico del servidor
            mostrarErrorModal(result.message || 'No se pudo identificar la planta');
        }
    } catch (error) {
        console.error('Error al identificar planta:', error);
        mostrarErrorModal(`Error de conexión: ${error.message}`);
    }
}

function mostrarCargando() {
    contenidoModal.innerHTML = `
        <div class="text-center py-12">
            <div class="animate-spin rounded-full h-16 w-16 border-b-2 border-[#1E6241] mx-auto mb-4"></div>
            <p class="text-lg text-gray-600">Identificando planta...</p>
        </div>
    `;
    modalIdentificacion.classList.remove('hidden');
}

function mostrarResultadoIdentificacion(data) {
    console.log('Datos recibidos:', data); // Debug

    const suggestions = data.result?.classification?.suggestions;

    if (!suggestions || suggestions.length === 0) {
        mostrarErrorModal('No se encontraron coincidencias para esta imagen');
        return;
    }

    const topSuggestion = suggestions[0];
    const probability = (topSuggestion.probability * 100).toFixed(1);

    // CORREGIDO: Usar snake_case para las propiedades de la API
    const commonNames = topSuggestion.details?.common_names?.length > 0
        ? topSuggestion.details.common_names.join(', ')
        : 'No disponible';

    const synonyms = topSuggestion.details?.synonyms?.length > 0
        ? topSuggestion.details.synonyms.slice(0, 3).join(', ')
        : 'No disponible';

    const similarImages = topSuggestion.similar_images || [];

    // Manejar información de riego
    const watering = topSuggestion.details?.watering;
    let wateringInfo = 'No disponible';

    if (watering) {
        const minValue = watering.min !== undefined && watering.min !== null
            ? watering.min.toString()
            : '';
        const maxValue = watering.max !== undefined && watering.max !== null
            ? watering.max.toString()
            : '';

        if (minValue && maxValue && minValue !== maxValue) {
            wateringInfo = `${minValue} - ${maxValue}`;
        } else if (minValue) {
            wateringInfo = minValue;
        } else if (maxValue) {
            wateringInfo = maxValue;
        } else if (minValue && maxValue && minValue === maxValue) {
            wateringInfo = minValue; // Si min y max son iguales, mostrar solo uno
        }
    }

    // Manejar imágenes similares - usar snake_case
    let imagen = '';
    if (similarImages.length > 0) {
        const firstImage = similarImages[0];
        imagen = firstImage.url_small || firstImage.url || '';
    }

    contenidoModal.innerHTML = `
        <div class="space-y-6">
            ${imagen ? `
                <img src="${imagen}" 
                     alt="${topSuggestion.name}" 
                     class="w-full h-64 object-cover rounded-2xl"
                     onerror="this.style.display='none'">
            ` : ''}
            
            <div>
                <h3 class="text-2xl font-bold text-gray-800 mb-2">${topSuggestion.name}</h3>
                <div class="flex items-center gap-2 mb-4">
                    <span class="bg-green-100 text-green-800 px-3 py-1 rounded-full text-sm font-medium">
                        ${probability}% de coincidencia
                    </span>
                </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="bg-gray-50 p-4 rounded-xl">
                    <div class="flex items-center gap-2 mb-2">
                        <i class="ph ph-plant text-[#1E6241] text-xl"></i>
                        <h4 class="font-semibold text-gray-700">Nombres Comunes</h4>
                    </div>
                    <p class="text-gray-600">${commonNames}</p>
                </div>

                <div class="bg-gray-50 p-4 rounded-xl">
                    <div class="flex items-center gap-2 mb-2">
                        <i class="ph ph-drop text-[#1E6241] text-xl"></i>
                        <h4 class="font-semibold text-gray-700">Riego</h4>
                    </div>
                    <p class="text-gray-600">${wateringInfo}</p>
                </div>
            </div>

            <div class="bg-gray-50 p-4 rounded-xl">
                <div class="flex items-center gap-2 mb-2">
                    <i class="ph ph-list text-[#1E6241] text-xl"></i>
                    <h4 class="font-semibold text-gray-700">Sinónimos</h4>
                </div>
                <p class="text-gray-600">${synonyms}</p>
            </div>

            ${suggestions.length > 1 ? `
                <div class="border-t pt-4">
                    <h4 class="font-semibold text-gray-700 mb-3">Otras posibles coincidencias:</h4>
                    <div class="space-y-2">
                        ${suggestions.slice(1, 4).map(s => `
                            <div class="flex justify-between items-center text-sm">
                                <span class="text-gray-600">${s.name}</span>
                                <span class="text-gray-500">${(s.probability * 100).toFixed(1)}%</span>
                            </div>
                        `).join('')}
                    </div>
                </div>
            ` : ''}

            <button onclick="location.reload()" 
                    class="w-full bg-[#1E6241] hover:bg-[#195136] text-white px-6 py-3 rounded-xl transition-colors duration-300">
                Buscar otra planta
            </button>
        </div>
    `;
}

function mostrarErrorModal(mensaje) {
    contenidoModal.innerHTML = `
        <div class="text-center py-8">
            <i class="ph ph-warning text-red-500 text-6xl mb-4"></i>
            <p class="text-lg text-gray-700 mb-6">${mensaje}</p>
            <button onclick="document.getElementById('modalIdentificacion').classList.add('hidden')" 
                    class="bg-[#1E6241] hover:bg-[#195136] text-white px-6 py-3 rounded-xl transition-colors duration-300">
                Cerrar
            </button>
        </div>
    `;
    modalIdentificacion.classList.remove('hidden');
}

// Cerrar modal
btnCerrarModal.addEventListener('click', () => {
    modalIdentificacion.classList.add('hidden');
});

modalIdentificacion.addEventListener('click', (e) => {
    if (e.target === modalIdentificacion) {
        modalIdentificacion.classList.add('hidden');
    }
});

// Cerrar con tecla ESC
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        modalIdentificacion.classList.add('hidden');
    }
});