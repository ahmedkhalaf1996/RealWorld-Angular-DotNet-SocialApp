describe("App Navigation", ()=>{
  it('should load home page', ()=>{
    cy.visit('/');
    cy.get('app-root').should('exist');
  })

  it('should load auth page', ()=>{
    cy.visit('/auth');
    cy.get('app-root').should('exist');
  })

    it('should load chat page', ()=>{
    cy.visit('/chat');
    cy.get('app-root').should('exist');
  })

    it('should load search page', ()=>{
    cy.visit('/search');
    cy.get('app-root').should('exist');
  })

    it('should load notificaion page', ()=>{
    cy.visit('/notification');
    cy.get('app-root').should('exist');
  })

    it('should load profile page', ()=>{
    cy.visit('/profile/1');
    cy.get('app-root').should('exist');
  })

})
