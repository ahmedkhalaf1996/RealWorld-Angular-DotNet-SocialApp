describe("Home page", ()=>{
  beforeEach(()=>{
    cy.visit("/");
  });

  it("should load the home page", ()=>{
    cy.get("app-root").should('exist');
  });

  it("should render with out  erros", ()=>{
    cy.get('body').should('exist');
  })

  it('Should have visible content', ()=>{
    cy.get('body').should('be.visible');
  })
})
