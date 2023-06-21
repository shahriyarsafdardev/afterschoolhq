import React, { useState, createContext, useContext } from 'react';

const UnsavedChangesContext = createContext();

export const UnsavedChangesProvider = ({ children }) => {
  const [unsavedChanges, setUnsavedChanges] = useState(false);

  const markChangesAsSaved = () => {
    setUnsavedChanges(false);
  };

  const markChangesAsUnsaved = () => {
    setUnsavedChanges(true);
  };

  return (
    <UnsavedChangesContext.Provider
      value={{ unsavedChanges, markChangesAsSaved, markChangesAsUnsaved }}
    >
      {children}
    </UnsavedChangesContext.Provider>
  );
};

export const useUnsavedChanges = () => {
  return useContext(UnsavedChangesContext);
};
