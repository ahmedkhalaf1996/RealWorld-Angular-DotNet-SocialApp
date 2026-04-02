describe("Chat page", ()=>{
  beforeEach(()=>{
    cy.visit("/chat");
  });

  it("should load the Chat page", ()=>{
    cy.get("app-root").should('exist');
  });

  it("should render with out  erros", ()=>{
    cy.get('body').should('exist');
  })
})
