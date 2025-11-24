const generarEmail = () => `test_${Date.now()}@ejemplo.com`;
const generarNombre = () => `Usuario Test ${Date.now()}`;

let emailUsuario = '';
let passwordUsuario = 'Password123!';
let nombreUsuario = '';

let emailUsuarioAdmin = 'admin@gmail.com';
let passwordUsuarioAdmin = 'administrador';

const plantaPrueba = {
    nombre: `Planta Test ${Date.now()}`,
    nombreCientifico: 'Testus Plantae',
    descripcion: 'Esta es una planta de prueba automatizada',
    cuidadosEspeciales: 'Regar con moderación',
    dificultad: 'baja',
    alturaMaxCm: 50,
    notas: 'Notas de prueba'
};

// Grupo 1: Registro de usuario
describe('Grupo 1: Registro de Usuario', () => {
    beforeEach(() => {
        cy.visit('https://localhost:7258/Auth/Registrar');
        cy.wait(500);
    });

    it('Debe cargar correctamente la página de registro', () => {
        cy.url().should('include', '/Auth/Registrar');
        cy.contains('h2', 'Crear Cuenta').should('be.visible');
        cy.get('input[name="Nombre"]').should('be.visible');
        cy.get('input[name="Email"]').should('be.visible');
        cy.get('input[name="Password"]').should('be.visible');
        cy.get('input[name="ConfirmPassword"]').should('be.visible');
        cy.wait(1000);
    });

    it('Debe mostrar errores de validación con campos vacíos', () => {
        cy.get('button[type="submit"]').click();
        cy.wait(500);
        cy.get('span.text-red-600').should('exist');
        cy.wait(500);
    });

    it('Debe mostrar error si las contraseñas no coinciden', () => {
        emailUsuario = generarEmail();
        nombreUsuario = generarNombre();

        cy.get('input[name="Nombre"]').type(nombreUsuario);
        cy.get('input[name="Email"]').type(emailUsuario);
        cy.get('input[name="Password"]').type('Password123!');
        cy.get('input[name="ConfirmPassword"]').type('Password456!');
        cy.get('button[type="submit"]').click();
        cy.wait(500);
        cy.get('#ConfirmPassword-error').should('exist');
        cy.wait(500);
    });

    it('Debe registrar un nuevo usuario exitosamente', () => {
        emailUsuario = generarEmail();
        nombreUsuario = generarNombre();

        cy.get('input[name="Nombre"]').type(nombreUsuario);
        cy.wait(500);
        cy.get('input[name="Email"]').type(emailUsuario);
        cy.wait(500);
        cy.get('input[name="Password"]').type(passwordUsuario);
        cy.wait(500);
        cy.get('input[name="ConfirmPassword"]').type(passwordUsuario);
        cy.wait(500);
        cy.get('button[type="submit"]').click();
        cy.wait(1000);
        
        cy.url().should('not.include', '/Registrar');
    });

});

// Grupo 2: Inicio de Sesión
describe('Grupo 2: Inicio de Sesión', () => {
    before(() => {
        emailUsuario = generarEmail();
        nombreUsuario = generarNombre();
        
        cy.visit('https://localhost:7258/Auth/Registrar');
        cy.wait(500);
        cy.get('input[name="Nombre"]').type(nombreUsuario);
        cy.get('input[name="Email"]').type(emailUsuario);
        cy.get('input[name="Password"]').type(passwordUsuario);
        cy.get('input[name="ConfirmPassword"]').type(passwordUsuario);
        cy.get('button[type="submit"]').click();
        cy.wait(1000);
        
        cy.visit('https://localhost:7258');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('form[action*="Logout"]').length > 0) {
                cy.get('form[action*="Logout"]').submit();
                cy.wait(500);
            }
        });
    });

    beforeEach(() => {
        cy.visit('https://localhost:7258/Auth/Login');
        cy.wait(500);
    });


    it('Debe cargar correctamente la página de login', () => {
        cy.url().should('include', '/Auth/Login');
        cy.contains('h2', 'ViveroApp').should('be.visible');
        cy.contains('p', 'Inicia sesión para continuar').should('be.visible');
        cy.get('input[name="Email"]').should('be.visible');
        cy.wait(500);
        cy.get('input[name="Password"]').should('be.visible');
        cy.get('input[name="RememberMe"]').should('exist');
        cy.wait(500);
    });

    it('Debe mostrar errores de validación con campos vacíos', () => {
        cy.get('button[type="submit"]').click();
        cy.wait(500);
        cy.get('span.text-red-600').should('exist');
    });

    it('Debe mostrar error con credenciales incorrectas', () => {
        cy.get('input[name="Email"]').type('usuario@noexiste.com');
        cy.get('input[name="Password"]').type('PasswordIncorrecto123!');
        cy.get('button[type="submit"]').click();
        cy.wait(1000);
        
        cy.url().should('include', '/Login');
    });

    it('Debe iniciar sesión exitosamente con credenciales correctas', () => {
        cy.get('input[name="Email"]').type(emailUsuario);
        cy.wait(200);
        cy.get('input[name="Password"]').type(passwordUsuario);
        cy.wait(200);
        cy.get('button[type="submit"]').click();
        cy.wait(1000);
        
        cy.url().should('not.include', '/Login');
        cy.url().should('include', 'localhost:7258');
    });



    it('Debe funcionar el checkbox de "Recordarme"', () => {

        cy.get('input[name="RememberMe"][type="checkbox"]').check();
        cy.wait(500);
        cy.get('input[name="RememberMe"][type="checkbox"]').should('be.checked');
        cy.wait(500);
    });

    it('Debe mostrar el enlace para registrarse', () => {
        cy.contains('a', 'Regístrate aquí').should('be.visible').and('have.attr', 'href');
        cy.wait(500);
    });
});

// Grupo 3: Panel Admin - Índice de Plantas
describe('Grupo 3: Panel Admin - Índice de Plantas', () => {

    beforeEach(() => {
        cy.visit('https://localhost:7258/Auth/Login');
        cy.wait(500);
        cy.get('input[name="Email"]').type(emailUsuarioAdmin);
        cy.get('input[name="Password"]').type(passwordUsuarioAdmin);
        cy.get('button[type="submit"]').click();
        cy.wait(1000);
        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
    });

    it('Debe cargar correctamente la página de gestión de plantas', () => {
        cy.url().should('include', '/Admin/Plantas');
        cy.contains('h2', 'Gestionar Plantas').should('be.visible');
        cy.wait(500);
    });

    it('Debe mostrar el botón de Nueva Planta', () => {
        cy.contains('a', 'Nueva Planta')
            .should('be.visible')
            .and('have.attr', 'href')
            .and('include', '/CrearPlanta');
        cy.wait(500);
    });

    it('Debe mostrar la tabla con las columnas correctas', () => {
        cy.get('table thead th').should('contain', 'Imagen');
        cy.get('table thead th').should('contain', 'Nombre');
        cy.get('table thead th').should('contain', 'Nombre Científico');
        cy.get('table thead th').should('contain', 'Dificultad');
        cy.get('table thead th').should('contain', 'Tóxica');
        cy.get('table thead th').should('contain', 'Acciones');
        cy.wait(500);
    });

    it('Debe mostrar el contador de plantas registradas', () => {
        cy.get('p.text-xl').should('contain', 'plantas registradas');
        cy.wait(500);
    });

    it('Debe poder navegar al formulario de creación', () => {
        cy.contains('a', 'Nueva Planta').click();
        cy.wait(500);
        cy.url().should('include', '/Admin/CrearPlanta');
        cy.wait(500);
    });
});

// Grupo 4: Crear Nueva Planta
describe('Grupo 4: Crear Nueva Planta', () => {

    beforeEach(() => {
        cy.visit('https://localhost:7258/Auth/Login');
        cy.wait(500);
        cy.get('input[name="Email"]').type(emailUsuarioAdmin);
        cy.get('input[name="Password"]').type(passwordUsuarioAdmin);
        cy.get('button[type="submit"]').click();
        cy.wait(1000);
        cy.visit('https://localhost:7258/Admin/CrearPlanta');
        cy.wait(500);
    });

    it('Debe cargar correctamente el formulario de creación', () => {
        cy.url().should('include', '/Admin/CrearPlanta');
        cy.wait(500);
        cy.contains('h2', 'Crear Nueva Planta').should('be.visible');
        cy.get('input[name="Nombre"]').should('be.visible');
        cy.wait(500);
        cy.get('input[name="NombreCientifico"]').should('be.visible');
        cy.get('textarea[name="Descripcion"]').should('be.visible');
        cy.get('select[name="Dificultad"]').should('be.visible');
        cy.wait(500);
    });

    it('Debe mostrar todos los campos requeridos', () => {
        cy.get('input[name="Nombre"]').should('exist');
        cy.wait(500);
        cy.get('input[name="NombreCientifico"]').should('exist');
        cy.get('textarea[name="Descripcion"]').should('exist');
        cy.wait(500);
        cy.get('textarea[name="CuidadosEspeciales"]').should('exist');
        cy.get('select[name="RiegoId"]').should('exist');
        cy.wait(500);
        cy.get('select[name="LuzId"]').should('exist');
        cy.get('select[name="SustratoId"]').should('exist');
        cy.wait(500);
        cy.get('select[name="Dificultad"]').should('exist');
        cy.get('input[name="AlturaMaxCm"]').should('exist');
        cy.wait(500);
        cy.get('input[name="Toxica"]').should('exist');
    });

    it('Debe mostrar errores de validación al enviar campos vacíos', () => {
        cy.contains('button', /Crear Planta/i).click();
        cy.wait(500);
        cy.get('span.text-red-600').should('exist');
        cy.wait(500);
    });

    it('Debe permitir agregar categorías', () => {
        cy.get('#categoriaSelect').select(1);
        cy.wait(500);
        cy.get('#categoriasSeleccionadas').children().should('have.length.greaterThan', 0);
    });

    it('Debe permitir eliminar categorías agregadas', () => {
        cy.get('#categoriaSelect').select(1);
        cy.wait(500);
        cy.get('#categoriasSeleccionadas button').first().click();
        cy.wait(500);
        cy.get('#categoriasSeleccionadas').children().should('have.length', 0);
    });

    it('Debe validar el checkbox de planta tóxica', () => {
        cy.get('input[name="Toxica"][type="checkbox"]').check();
        cy.wait(500);
        cy.get('input[name="Toxica"][type="checkbox"]').should('be.checked');
        cy.wait(500);
        cy.get('input[name="Toxica"][type="checkbox"]').uncheck();
        cy.wait(500);
        cy.get('input[name="Toxica"][type="checkbox"]').should('not.be.checked');
        cy.wait(500);
    });

    it('Debe funcionar el botón de cancelar', () => {
        cy.contains('a', 'Cancelar').click();
        cy.wait(500);
        cy.url().should('include', '/Admin/Plantas');
        cy.wait(500);
    });

    it('Debe crear una planta exitosamente con datos válidos', () => {
        const nombrePlanta = `Planta Test ${Date.now()}`;
        
        cy.get('input[name="Nombre"]').type(nombrePlanta);
        cy.wait(500);
        cy.get('input[name="NombreCientifico"]').type(plantaPrueba.nombreCientifico);
        cy.wait(500);
        cy.get('textarea[name="Descripcion"]').type(plantaPrueba.descripcion);
        cy.wait(500);
        cy.get('textarea[name="CuidadosEspeciales"]').type(plantaPrueba.cuidadosEspeciales);
        cy.wait(500);
        
        cy.get('select[name="RiegoId"]').select(1);
        cy.wait(500);
        cy.get('select[name="LuzId"]').select(1);
        cy.wait(500);
        cy.get('select[name="SustratoId"]').select(1);
        cy.wait(500);
        cy.get('select[name="Dificultad"]').select(plantaPrueba.dificultad);
        cy.wait(500);
        
        cy.get('input[name="AlturaMaxCm"]').type(plantaPrueba.alturaMaxCm.toString());
        cy.wait(500);
        cy.get('textarea[name="Notas"]').type(plantaPrueba.notas);
        cy.wait(500);
        
        cy.get('#categoriaSelect').select(1);
        cy.wait(500);
        
        cy.contains('button', /Crear Planta/i).click();
        cy.wait(1500);
        
        cy.url().should('include', '/Admin/Plantas');
        cy.contains('td', nombrePlanta).should('be.visible');
    });

});

// Grupo 5: Editar Planta
describe('Grupo 5: Editar Planta', () => {
    let plantaId;
    let nombrePlantaOriginal;

    beforeEach(() => {
        nombrePlantaOriginal = `Planta Para Editar ${Date.now()}`;
        
        cy.visit('https://localhost:7258/Auth/Login');
        cy.wait(500);
        cy.get('input[name="Email"]').type(emailUsuarioAdmin);
        cy.get('input[name="Password"]').type(passwordUsuarioAdmin);
        cy.get('button[type="submit"]').click();
        cy.wait(1000);
        
        cy.visit('https://localhost:7258/Admin/CrearPlanta');
        cy.wait(500);
        
        cy.get('input[name="Nombre"]').type(nombrePlantaOriginal);
        cy.get('input[name="NombreCientifico"]').type('Plantae Editae');
        cy.wait(500);
        cy.get('textarea[name="Descripcion"]').type('Descripción para editar');
        cy.wait(500);
        cy.get('select[name="RiegoId"]').select(1);
        cy.wait(500);
        cy.get('select[name="LuzId"]').select(1);
        cy.wait(500);
        cy.get('select[name="SustratoId"]').select(1);
        cy.wait(500);
        cy.get('select[name="Dificultad"]').select('media');
        cy.wait(500);
        cy.get('input[name="AlturaMaxCm"]').type('30');
        cy.wait(500);
        cy.contains('button', /Crear Planta/i).click();
        cy.wait(1500);
        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        
        cy.contains('td', nombrePlantaOriginal)
            .parent('tr')
            .find('a[href*="EditarPlanta"]')
            .invoke('attr', 'href')
            .then(href => {
                plantaId = href.match(/\d+/)[0];
                cy.visit(`https://localhost:7258${href}`);
                cy.wait(500);
            });
    });

    it('Debe cargar correctamente el formulario de edición', () => {
        cy.url().should('include', '/Admin/EditarPlanta');
        cy.contains('h2', 'Editar Planta').should('be.visible');
        cy.wait(500);
        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });

    });

    it('Debe cargar los datos actuales de la planta', () => {
        cy.get('input[name="Nombre"]').should('have.value', nombrePlantaOriginal);
        cy.get('input[name="NombreCientifico"]').should('have.value', 'Plantae Editae');
        cy.get('textarea[name="Descripcion"]').should('have.value', 'Descripción para editar');
        cy.wait(500);
        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });
    
    });

    it('Debe permitir cambiar el nombre de la planta', () => {
        const nuevoNombre = `Planta Editada ${Date.now()}`;
        
        cy.get('input[name="Nombre"]').clear().type(nuevoNombre);
        cy.wait(200);
        cy.contains('button', /Guardar Cambios/i).click();
        cy.wait(1500);
        
        cy.url().should('include', '/Admin/Plantas');
        cy.contains('td', nuevoNombre).should('be.visible');

        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });

    });


    it('Debe permitir cambiar la dificultad', () => {
        cy.get('select[name="Dificultad"]').select('alta');
        cy.wait(500);
        cy.get('select[name="Dificultad"]').should('have.value', 'alta');
        cy.wait(500);

        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });

    });


    it('Debe permitir modificar la altura máxima', () => {
        cy.get('input[name="AlturaMaxCm"]').clear().type('100');
        cy.wait(500);
        cy.get('input[name="AlturaMaxCm"]').should('have.value', '100');
        cy.wait(500);

        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });
    });

    it('Debe permitir cambiar el estado de tóxica', () => {
        cy.get('input[name="Toxica"][type="checkbox"]').check();
        cy.wait(500);
        cy.get('input[name="Toxica"][type="checkbox"]').should('be.checked');
        cy.wait(500);
        
        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });

    });

    it('Debe mostrar las categorías actuales de la planta', () => {
        cy.get('#categoriasSeleccionadas').should('exist');
        cy.wait(500);
        
        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });

    });

    it('Debe permitir agregar nuevas categorías', () => {
        cy.get('#categoriaSelect option:not(:disabled)').then($options => {
            if ($options.length > 1) {
                cy.get('#categoriaSelect').select(1);
                cy.wait(300);
                cy.get('#categoriasSeleccionadas').children().should('have.length.greaterThan', 0);
            }
        });
        
        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });

    });

    it('Debe funcionar el botón de cancelar', () => {
        cy.contains('a', 'Cancelar').click();
        cy.wait(500);
        cy.url().should('include', '/Admin/Plantas');

        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });

    });

    it('Debe mostrar validación al intentar enviar campos requeridos vacíos', () => {
        cy.get('input[name="Nombre"]').clear();
        cy.wait(500);
        cy.contains('button', /Guardar Cambios/i).click();
        cy.wait(500);
        cy.get('span.text-red-600').should('exist');

        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });

    });

    it('Debe guardar todos los cambios correctamente', () => {
        const nombreEditado = `Planta Totalmente Editada ${Date.now()}`;
        
        cy.get('input[name="Nombre"]').clear().type(nombreEditado);
        cy.wait(200);
        cy.get('input[name="NombreCientifico"]').clear().type('Plantae Totalus Editae');
        cy.wait(200);
        cy.get('textarea[name="Descripcion"]').clear().type('Descripción completamente actualizada');
        cy.wait(200);
        cy.get('select[name="Dificultad"]').select('baja');
        cy.wait(200);
        cy.get('input[name="AlturaMaxCm"]').clear().type('75');
        cy.wait(200);
        cy.get('input[name="Toxica"][type="checkbox"]').check();
        cy.wait(200);
        
        cy.contains('button', /Guardar Cambios/i).click();
        cy.wait(1500);
        
        cy.url().should('include', '/Admin/Plantas');
        cy.contains('td', nombreEditado).should('be.visible');

        cy.visit('https://localhost:7258/Admin/Plantas');
        cy.wait(500);
        cy.get('body').then($body => {
            if ($body.find('td').filter(`:contains("Planta")`).length > 0) {
                cy.contains('td', 'Planta').first().parent('tr').find('button[type="submit"]').click();
                cy.wait(500);
            }
        });
    });
});

