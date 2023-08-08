import cs from 'classnames';
import React, { useState, useEffect } from 'react';
import { Typography, Box, Button } from '@material-ui/core';
import { ArrowDropDownOutlined as ArrowDropDownOutlinedIcon } from '@material-ui/icons';

import RenderAudienceList from './RenderAudienceList';
import useStyles from './styles';
import Dialog from './Dialog';

const ApplyToAllControls = props => {
  const classes = useStyles();
  const [showAudience, setShowAudience] = useState(false);
  const { allHide, values, setValues, reRender, setReRender, settingsForAll } = props;
  const [isHide, setIsHide] = useState(allHide);
  const [isOpen, setIsOpen] = useState(false);

  useEffect(() => setIsHide(allHide), [allHide]);

  const handleSettingForAll = ({ settings, willDelete, willHide }) => {
    const questionKeys = [];
    let newValues = { ...values };

    const updates = () => {
      if (willHide !== undefined) return { IsActive: willHide };
      if (willDelete !== undefined) return { IsDeleted: willDelete };
      if (settings) return { QuestionSettings: settings };
    };

    Object.keys(newValues).forEach(key => {
      if (newValues[key].IsLocked || !newValues[key].checked) {
        return;
      }

      questionKeys.push(key);
      newValues[key] = {
        ...newValues[key],
        ...updates()
      };
    });

    if (willDelete) newValues = handleDeletion(newValues, questionKeys);
    setReRender(!reRender);
    setValues(newValues, false);
  };

  const handleDeletion = (values, keys) => {
    let index = 1;
    const newValues = {};
    let deletedIndex = Object.keys(values).length - keys.length + 1;

    Object.keys(values).forEach(key => {
      if (keys.includes(key)) {
        if (values[key].ID == 0) return;
        newValues[deletedIndex++] = { ...values[key], checked: false };
      } else newValues[index++] = values[key];
    });
    return newValues;
  };

  const handleHideSection = () => {
    handleSettingForAll({ willHide: isHide });
    setIsHide(!isHide);
  };

  const handleCloseDialog = () => {
    setIsOpen(false);
  };

  const handleDeleteSection = () => {
    handleSettingForAll({ willDelete: true });
    handleCloseDialog();
  };

  return (
    <Box className={classes.applyAllBox}>
      <Typography variant="h6" className={classes.applyAllHeading}>
        Apply to All Selected:
      </Typography>
      <Box className={classes.setAudienceWrapper}>
        <Button
          variant="outlined"
          className={cs(classes.audienceButton, classes.setAudienceButton)}
          onClick={() => setShowAudience(!showAudience)}
          data-testid="setAudienceButtonForAll"
        >
          Set Audience
          <ArrowDropDownOutlinedIcon className={classes.dropDownIcon} />
        </Button>
        {showAudience && (
          <RenderAudienceList
            setFieldValue={settingsForAll => handleSettingForAll({ settings: settingsForAll })}
            questionSettings={settingsForAll}
            setShowAudience={setShowAudience}
            dataTestId="audienceListForAll"
            styleClass={classes.displayAudience}
          />
        )}
      </Box>
      <Button
        color="primary"
        variant="outlined"
        className={classes.applyToAllButton}
        onClick={handleHideSection}
        data-testid="hideButtonForAll"
      >
        {isHide ? 'UnHide' : 'Hide'}
      </Button>
      <Button
        color="primary"
        variant="outlined"
        className={classes.applyToAllButton}
        onClick={() => setIsOpen(true)}
        data-testid="deleteButtonForAll"
      >
        Delete
      </Button>
      <Dialog
        open={isOpen}
        onClose={handleCloseDialog}
        onSubmit={handleDeleteSection}
        title="Delete Questions"
        text="Do you want to delete the selected questions? This action is irreversible."
        dataTestId="deleteSelectedQuestions"
      />
    </Box>
  );
};

export default ApplyToAllControls;
