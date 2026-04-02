describe("auth page", ()=>{
  beforeEach(()=>{
    cy.visit("/auth");
  });

  it("should load the auth page", ()=>{
    cy.get("app-root").should('exist');
  });

  it("should render with out  erros", ()=>{
    cy.get('body').should('exist');
  })
})
