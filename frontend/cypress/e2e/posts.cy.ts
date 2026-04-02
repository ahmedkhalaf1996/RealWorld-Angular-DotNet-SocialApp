describe("Posts page", ()=>{
  beforeEach(()=>{
    cy.visit("/");
  });

  it("should load the Posts page", ()=>{
    cy.get("app-root").should('exist');
  });

  it("should render with out  erros", ()=>{
    cy.get('body').should('exist');
  })
})
